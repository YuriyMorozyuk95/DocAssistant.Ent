// Copyright (c) Microsoft. All rights reserved.

using ClientApp.Services;
using Shared.TableEntities;

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

        api.MapUserManagementApi();
        api.MapPermissionManagementApi();

        return app;
    }

    private static async Task<IResult> OnPostCopilotPromptsAsync(HttpContext context, [FromServices] ILogger<CopilotPromptsRequestResponse> logger)
    {
        logger.LogInformation("Write prompt files");

        var updatedData = await context.Request.ReadFromJsonAsync<CopilotPromptsRequestResponse>();
        if (updatedData != null)
        {
            PromptFileService.UpdatePromptsToFile(PromptFileNames.CreateAnswer, updatedData.CreateAnswer);
            PromptFileService.UpdatePromptsToFile(PromptFileNames.CreateJsonPrompt, updatedData.CreateJsonPrompt);
            PromptFileService.UpdatePromptsToFile(PromptFileNames.CreateJsonPrompt2, updatedData.CreateJsonPrompt2);
            PromptFileService.UpdatePromptsToFile(PromptFileNames.SearchPrompt, updatedData.SearchPrompt);
            PromptFileService.UpdatePromptsToFile(PromptFileNames.SystemFollowUp, updatedData.SystemFollowUp);
            PromptFileService.UpdatePromptsToFile(PromptFileNames.SystemFollowUpContent, updatedData.SystemFollowUpContent);
        }

        return TypedResults.Ok();
    }

    private static CopilotPromptsRequestResponse OnGetCopilotPrompts([FromServices] ILogger<CopilotPromptsRequestResponse> logger)
    {
        logger.LogInformation("Load file contents documents");

        var response = new CopilotPromptsRequestResponse
        {
            CreateAnswer = PromptFileService.ReadPromptsFromFile(PromptFileNames.CreateAnswer),
            CreateJsonPrompt = PromptFileService.ReadPromptsFromFile(PromptFileNames.CreateJsonPrompt),
            CreateJsonPrompt2 = PromptFileService.ReadPromptsFromFile(PromptFileNames.CreateJsonPrompt2),
            SearchPrompt = PromptFileService.ReadPromptsFromFile(PromptFileNames.SearchPrompt),
            SystemFollowUp = PromptFileService.ReadPromptsFromFile(PromptFileNames.SystemFollowUp),
            SystemFollowUpContent = PromptFileService.ReadPromptsFromFile(PromptFileNames.SystemFollowUpContent),
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
        [FromForm] string permissions,
        [FromServices] AzureBlobStorageService service,
        [FromServices] ILogger<AzureBlobStorageService> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Upload documents");

        // Deserialize permissions from JSON
        var deserializedPermissions = JsonSerializer.Deserialize<PermissionEntity[]>(permissions);

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
        await service.UploadToAzureIndex();
        return TypedResults.Ok();
    }
}
