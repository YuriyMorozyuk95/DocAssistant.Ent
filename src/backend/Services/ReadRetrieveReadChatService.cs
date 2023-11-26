﻿// Copyright (c) Microsoft. All rights reserved.

using ClientApp.Components;

using Microsoft.Identity.Client;

namespace MinimalApi.Services;

public class ReadRetrieveReadChatService
{
    private readonly SearchClient _searchClient;
    private readonly IKernel _kernel;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReadRetrieveReadChatService> _logger;

    public ReadRetrieveReadChatService(
        SearchClient searchClient,
        OpenAIClient client,
        IConfiguration configuration,
        ILogger<ReadRetrieveReadChatService> logger)
    {
        _searchClient = searchClient;

        // Get the deployed model name from configuration  
        var deployedModelName = configuration["AzureOpenAiChatGptDeployment"];
        // ReSharper disable once AccessToStaticMemberViaDerivedType
        ArgumentNullException.ThrowIfNullOrWhiteSpace(deployedModelName);

        // Build the kernel with Azure Chat Completion Service  
        var kernelBuilder = Kernel.Builder.WithAzureChatCompletionService(deployedModelName, client);

        // If embedding model name is provided in configuration, add Text Embedding Generation Service to the kernel  
        var embeddingModelName = configuration["AzureOpenAiEmbeddingDeployment"];
        if (!string.IsNullOrEmpty(embeddingModelName))
        {
            var (azureOpenAiServiceEndpoint, key) = (configuration["AzureOpenAiServiceEndpoint"], configuration["AzureOpenAiServiceEndpointKey"]);
            // ReSharper disable once AccessToStaticMemberViaDerivedType
            ArgumentNullException.ThrowIfNullOrWhiteSpace(azureOpenAiServiceEndpoint);

            kernelBuilder = kernelBuilder.WithAzureTextEmbeddingGenerationService(embeddingModelName, azureOpenAiServiceEndpoint, key!);
        }

        _kernel = kernelBuilder.Build();
        _configuration = configuration;
        _logger = logger;
    }

    // This method generates a reply to a given chat history.  
    public async Task<ApproachResponse> ReplyAsync(
        ChatTurn[] history,
        RequestOverrides? overrides,
        CancellationToken cancellationToken = default)
    {
        // Get the top results, whether to use semantic captions and ranker, and the category to exclude from overrides
        //TODO permission & category
        var top = overrides?.Top ?? 3;
        var useSemanticCaptions = overrides?.SemanticCaptions ?? false;
        var useSemanticRanker = overrides?.SemanticRanker ?? false;
        var excludeCategory = overrides?.ExcludeCategory ?? null;
        var filter = excludeCategory is null ? null : $"category ne '{excludeCategory}'";

        // Get chat completion and text embedding generation services from the kernel  
        IChatCompletion chat = _kernel.GetService<IChatCompletion>();
        ITextEmbeddingGeneration? embedding = _kernel.GetService<ITextEmbeddingGeneration>();

        // If retrieval mode is not "Text" and embedding is not null, generate embeddings for the question 
        string question = GetQuestionFromHistory(history);

        float[]? embeddings = await GenerateEmbeddingsAsync(overrides, cancellationToken, embedding, question);

        // step 1
        // use llm to get query if retrieval mode is not vector
        // If retrieval mode is not "Vector", generate a search query using the chat completion service  
        string? query = await GenerateQueryAsync(overrides, cancellationToken, chat, question);

        // step 2
        // use query to search related docs
        // Use the search query to search related documents
        var documentContentList = await GetQueryDocuments(overrides, cancellationToken, query, embeddings);
        string documentContents = GetDocumentContents(documentContentList);

        // step 3
        // put together related docs and conversation history to generate answer
        // Create a new chat to generate the answer  
        var answerChat = CreateAnswerChat(history, chat, documentContents);

        // get answer
        // Get chat completions to generate the answer  
        (string answer, string thoughts) = await GetAnswerAsync(cancellationToken, chat, answerChat);

        // step 4
        // add follow up questions if requested
        // If follow-up questions are requested, generate them  
        if (overrides?.SuggestFollowupQuestions is true)
        {
            answer = await UpdateAnswerWithFollowUpQuestionsAsync(cancellationToken, chat, answer);
        }

        // Return the response  
        return new ApproachResponse(
            DataPoints: documentContentList,
            Answer: answer,
            Thoughts: thoughts,
            CitationBaseUrl: _configuration.ToCitationBaseUrl());
    }

    private async Task<string> UpdateAnswerWithFollowUpQuestionsAsync(CancellationToken cancellationToken, IChatCompletion chat, string answer)
    {
        var answerWithFollowUpQuestion = new string(answer);

        var systemFollowUp = PromptFileService.ReadPromptsFromFile("system-follow-up.txt");
        var systemFollowContent = PromptFileService.ReadPromptsFromFile("system-follow-up.txt",new Dictionary<string, string>
        {
            { "{answer}", answer }
        });

        var followUpQuestionChat = chat.CreateNewChat(systemFollowUp);
        _logger.LogInformation("system-follow-up: {x}", systemFollowUp);

        followUpQuestionChat.AddUserMessage(systemFollowContent);
        _logger.LogInformation("system-follow-up-content: {x}", systemFollowContent);


        // Get chat completions to generate the follow-up questions  
        var followUpQuestions = await chat.GetChatCompletionsAsync(
            followUpQuestionChat,
            cancellationToken: cancellationToken);

        // Extract the follow-up questions from the result and add them to the answer  
        var followUpQuestionsJson = followUpQuestions[0].ModelResult.GetOpenAIChatResult().Choice.Message.Content;
        _logger.LogInformation("followUpQuestionsJson: {x}", followUpQuestionsJson);

        var followUpQuestionsObject = JsonSerializer.Deserialize<JsonElement>(followUpQuestionsJson);
        var followUpQuestionsList = followUpQuestionsObject.EnumerateArray().Select(x => x.GetString()).ToList();
        foreach (var followUpQuestion in followUpQuestionsList)
        {
            answerWithFollowUpQuestion += $" <<{followUpQuestion}>> ";
        }

        return answerWithFollowUpQuestion;
    }

    private async Task<(string answer, string thoughts)> GetAnswerAsync(CancellationToken cancellationToken, IChatCompletion chat, ChatHistory answerChat)
    {
        var answer = await chat.GetChatCompletionsAsync(
            answerChat,
            cancellationToken: cancellationToken);

        // Extract the answer and thoughts from the result  
        var answerJson = answer[0].ModelResult.GetOpenAIChatResult().Choice.Message.Content;
        var answerObject = JsonSerializer.Deserialize<JsonElement>(answerJson);

        var ans = answerObject.GetProperty("answer").GetString() ?? throw new InvalidOperationException("Failed to get answer");
        var thoughts = answerObject.GetProperty("thoughts").GetString() ?? throw new InvalidOperationException("Failed to get thoughts");
        return (ans, thoughts);
    }

    private ChatHistory CreateAnswerChat(ChatTurn[] history, IChatCompletion chat, string documentContents)
    {
        var createAnswerPrompt = PromptFileService.ReadPromptsFromFile("create-answer.txt");
        _logger.LogInformation("create-answer: {x}", createAnswerPrompt);

        var answerChat = chat.CreateNewChat(createAnswerPrompt);

        // add chat history
        foreach (var turn in history)
        {
            answerChat.AddUserMessage(turn.User);
            if (turn.Bot is { } botMessage)
            {
                answerChat.AddAssistantMessage(botMessage);
                _logger.LogInformation("history: {x}", botMessage);
            }
        }


        var createJsonPrompt = PromptFileService.ReadPromptsFromFile("create-json-prompt.txt", new Dictionary<string, string>
        {
            { "{documentContents}", documentContents }
        });
        _logger.LogInformation("create-json-prompt: {x}", createJsonPrompt);
        // format prompt
        // Add the document contents and the answer format to the chat  
        answerChat.AddUserMessage(createJsonPrompt);
        return answerChat;
    }

    private string GetDocumentContents(SupportingContentRecord[] documentContentList)
    {
        string documentContents =
            // Join document contents or set as "no source available" if no documents found  
            documentContentList.Length == 0 ? "no source available." : string.Join("\r", documentContentList.Select(x => $"{x.Title}:{x.Content}"));

        // Print document contents to the console  
        _logger.LogInformation(documentContents);
        return documentContents;
    }

    private Task<SupportingContentRecord[]> GetQueryDocuments(RequestOverrides? overrides, CancellationToken cancellationToken, string? query, float[]? embeddings)
    {
        return _searchClient.QueryDocumentsAsync(query, embeddings, overrides, cancellationToken);
    }

    private async Task<string?> GenerateQueryAsync(RequestOverrides? overrides, CancellationToken cancellationToken, IChatCompletion chat, string question)
    {
        string? query = null;
        if (overrides?.RetrievalMode != "Vector")
        {
            var searchPrompt = PromptFileService.ReadPromptsFromFile("search-prompt.txt");
            // Create a new chat to generate the search query  
            var getQueryChat = chat.CreateNewChat(searchPrompt);

            // Add the user question to the chat 
            getQueryChat.AddUserMessage(question);
            var result = await chat.GetChatCompletionsAsync(
                getQueryChat,
                cancellationToken: cancellationToken);
            _logger.LogInformation("searchPrompt: {x}", searchPrompt);
            _logger.LogInformation("question: {x}", question);

            // If no result is returned, throw an exception 
            if (result.Count != 1)
            {
                throw new InvalidOperationException("Failed to get search query");
            }

            // Extract the search query from the result  
            query = result[0].ModelResult.GetOpenAIChatResult().Choice.Message.Content;
            _logger.LogInformation("Query: {x}", query);
        }

        return query;
    }

    private async Task<float[]?> GenerateEmbeddingsAsync(RequestOverrides? overrides, CancellationToken cancellationToken, ITextEmbeddingGeneration embedding, string question)
    {
        float[]? embeddings = null;
        if (overrides?.RetrievalMode != "Text" && embedding is not null)
        {
            embeddings = (await embedding.GenerateEmbeddingAsync(question, cancellationToken: cancellationToken)).ToArray();
        }

        return embeddings;
    }

    private string GetQuestionFromHistory(ChatTurn[] history)
    {
        var question = history.LastOrDefault()?.User is { } userQuestion
            ? userQuestion
            : throw new InvalidOperationException("Use question is null");
        return question;
    }
}
