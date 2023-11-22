// Copyright (c) Microsoft. All rights reserved.

namespace MinimalApi.Services;

internal sealed class AzureBlobStorageService
{
    private readonly IStorageService _storageService;

    public AzureBlobStorageService(IStorageService storageService)
    {
        _storageService = storageService;
    }
    internal async Task<UploadDocumentsResponse> UploadFilesAsync(IEnumerable<IFormFile> files, CancellationToken cancellationToken)
    {
        var container = await _storageService.GetInputBlobContainerClient();

        try
        {
            List<string> uploadedFiles = new();

            foreach (var file in files)
            {
                var fileName = file.FileName;
                await using var stream = file.OpenReadStream();

                var blobClient = container.GetBlobClient(fileName);

                if (await blobClient.ExistsAsync(cancellationToken))
                {
                    continue;
                }

                await blobClient.UploadAsync(
                    stream,
                    new BlobHttpHeaders
                    {
                        ContentType = "application/pdf"
                    },
                    cancellationToken: cancellationToken);

                uploadedFiles.Add(fileName);
            }

            if (uploadedFiles.Count is 0)
            {
                return UploadDocumentsResponse.FromError("""
                    No files were uploaded. Either the files already exist or the files are not PDFs.
                    """);
            }

            return new UploadDocumentsResponse(uploadedFiles.ToArray());
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
        {
            return UploadDocumentsResponse.FromError(ex.ToString());
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }
}
