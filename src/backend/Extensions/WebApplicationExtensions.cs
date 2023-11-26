// Copyright (c) Microsoft. All rights reserved.

using ClientApp.Services;
namespace MinimalApi.Extensions;

internal static class WebApplicationExtensions
{
    internal static WebApplication MapApi(this WebApplication app)
    {
        var api = app.MapGroup("api");

        // Long-form chat w/ contextual history endpoint
        api.MapPost("chat", OnPostChatAsync);

        // Upload a document
        api.MapPost("documents", OnPostDocumentAsync);

        // Get all documents
        api.MapGet("documents", OnGetDocumentsAsync);

        // synchronize documents in with blob storage and index
        api.MapPost("synchronize", OnPostSynchronizeAsync);

        api.MapGet("enableLogout", OnGetEnableLogout);

        api.MapGet("copilot-prompts", OnGetCopilotPrompts);  
  
        api.MapPost("copilot-prompts", OnPostCopilotPromptsAsync);  


        return app;
    }

    private static async Task<IResult> OnPostCopilotPromptsAsync(HttpContext context, [FromServices] ILogger<AzureBlobStorageService> logger)
    {
        logger.LogInformation("Write prompt files");

        var updatedData = await context.Request.ReadFromJsonAsync<CopilotPromptsRequestResponse>();
        if (updatedData != null)
        {
            PromptFileService.UpdatePromptsToFile("create-answer.txt", updatedData.CreateAnswer);
            PromptFileService.UpdatePromptsToFile("create-json-prompt.txt", updatedData.CreateJsonPrompt);
            PromptFileService.UpdatePromptsToFile("search-prompt.txt", updatedData.SearchPrompt);
            PromptFileService.UpdatePromptsToFile("system-follow-up.txt", updatedData.SystemFollowUp);
            PromptFileService.UpdatePromptsToFile("system-follow-up-content.txt", updatedData.SystemFollowUpContent);
        }

        return TypedResults.Ok();
    }

    private static CopilotPromptsRequestResponse OnGetCopilotPrompts([FromServices] ILogger<AzureBlobStorageService> logger)
    {
        logger.LogInformation("Load file contents documents");

        var response = new CopilotPromptsRequestResponse
        {
            CreateAnswer = PromptFileService.ReadPromptsFromFile("create-answer.txt"),
            CreateJsonPrompt = PromptFileService.ReadPromptsFromFile("create-json-prompt.txt"),
            SearchPrompt = PromptFileService.ReadPromptsFromFile("search-prompt.txt"),
            SystemFollowUp = PromptFileService.ReadPromptsFromFile("system-follow-up.txt"),
            SystemFollowUpContent = PromptFileService.ReadPromptsFromFile("system-follow-up-content.txt"),
        };

        return response;
    }

    private static IResult OnGetEnableLogout(HttpContext context)
    {
        var header = context.Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"];
        var enableLogout = !string.IsNullOrEmpty(header);

        return TypedResults.Ok(enableLogout);
    }

    private static async Task<IResult> OnPostChatAsync(
        ChatRequest request,
        ReadRetrieveReadChatService chatService,
        CancellationToken cancellationToken)
    {
        if (request is { History.Length: > 0 })
        {
            var response = await chatService.ReplyAsync(
                request.History, request.Overrides, cancellationToken);

            return TypedResults.Ok(response);
        }

        return Results.BadRequest();
    }

    private static async Task<IResult> OnPostDocumentAsync(
        [FromForm] IFormFileCollection files,
        [FromServices] AzureBlobStorageService service,
        [FromServices] ILogger<AzureBlobStorageService> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Upload documents");

        var response = await service.UploadFilesAsync(files, cancellationToken);

        logger.LogInformation("Upload documents: {x}", response);

        return TypedResults.Ok(response);
    }

    private static IAsyncEnumerable<DocumentResponse> OnGetDocumentsAsync(
        [FromServices] IUploaderDocumentService service,
        CancellationToken cancellationToken)
    {
        var doc = service.GetDocuments(cancellationToken);
        return doc;
    }

    private static async Task<IResult> OnPostSynchronizeAsync(
        [FromServices] IUploaderDocumentService service,
        CancellationToken cancellationToken)
    {
        await service.UploadToAzureIndex(cancellationToken);
        return TypedResults.Ok();
    }
}
