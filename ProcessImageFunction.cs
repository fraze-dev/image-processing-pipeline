using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using ImageProcessingPipeline.Models;
using ImageProcessingPipeline.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ImageProcessingPipeline;

public class ProcessImageFunction
{
    private readonly ILogger<ProcessImageFunction> _logger;
    private readonly ImageProcessor _imageProcessor;
    private readonly string _storageConnectionString;

    public ProcessImageFunction(ILogger<ProcessImageFunction> logger)
    {
        _logger = logger;
        _imageProcessor = new ImageProcessor();
        _storageConnectionString = Environment.GetEnvironmentVariable("StorageConnectionString")
            ?? throw new InvalidOperationException("StorageConnectionString not found");
    }


    [Function(nameof(ProcessImageFunction))]
    public async Task Run([BlobTrigger("uploads/{name}", Connection = "StorageConnectionString")] Stream imageStream, string name)
    {
        _logger.LogInformation($"Processing blob: {name}, Size: {imageStream.Length} bytes");

        try
        {
            //Reset stream position
            imageStream.Position = 0;

            //Get original dimensions
            var (originalWidth, originalHeight) = _imageProcessor.GetImageDimensions(imageStream);
            //Reset stream position again after getting dimension
            imageStream.Position = 0;

            //Resize image
            var (processedImage, processedWidth, processedHeight) =
                await _imageProcessor.ResizeImageAsync(imageStream);

            //Reset stream position again for thumbnail
            imageStream.Position = 0;
            var (thumbnail, thumbWidth, thumbHeight) =
                await _imageProcessor.CreateThumbnailAsync(imageStream);

            //Upload processed image to blob storage
            var blobServiceClient = new BlobServiceClient(_storageConnectionString);
            var processedContainerClient = blobServiceClient.GetBlobContainerClient("processed");
            var thumbnailContainerClient = blobServiceClient.GetBlobContainerClient("thumbnails");

            var processedFileName = $"processed_{name}";
            var thumbnailFileName = $"thumb_{name}";

            var processedBlobClient = processedContainerClient.GetBlobClient(processedFileName);
            await processedBlobClient.UploadAsync(
                    new MemoryStream(processedImage),
                    overwrite: true);
            
            var thumbnailBlobClient = thumbnailContainerClient.GetBlobClient(thumbnailFileName);
            await thumbnailBlobClient.UploadAsync(
                new MemoryStream(thumbnail),
                overwrite: true);

            //Save the metadata to table Storage
            var metadata = new ImageMetadata
            {
                OriginalFileName = name,
                ProcessedFileName = processedFileName,
                ThumbnailFileName = thumbnailFileName,
                OriginalSizeBytes = imageStream.Length,
                ProcessedSizeBytes = processedImage.Length,
                OriginalWidth = originalWidth,
                OriginalHeight = originalHeight,
                ProcessedWidth = processedWidth,
                ProcessedHeight = processedHeight,
                ProcessingStatus = "Completed",
                UploadedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            };

            var tableServiceClient = new TableServiceClient(_storageConnectionString);
            var tableClient = tableServiceClient.GetTableClient("ImageMetadata");
            await tableClient.CreateIfNotExistsAsync();
            await tableClient.AddEntityAsync(metadata);

            //Send notification to queue
            var queueServiceClient = new QueueServiceClient(_storageConnectionString);
            var queueClient = queueServiceClient.GetQueueClient("image-processing-queue");
            await queueClient.CreateIfNotExistsAsync();

            var message = new
            {
                FileName = name,
                Status = "Success",
                ProcessedAt = DateTime.UtcNow,
                OriginalSize = imageStream.Length,
                ProcessedSize = processedImage.Length
            };

            await queueClient.SendMessageAsync(JsonSerializer.Serialize(message));

            _logger.LogInformation($"Successfully processed image: {name}");
            
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing image {name}: {ex.Message}");

            //Send notice of failure to queue
            var queueServiceClient = new QueueServiceClient(_storageConnectionString);
            var queueClient = queueServiceClient.GetQueueClient("image-processing-queue");

            var errorMessage = new
            {
                FileName = name,
                Status = "Failed",
                Error = ex.Message,
                ProcessedAt = DateTime.UtcNow
            };

            await queueClient.SendMessageAsync(JsonSerializer.Serialize(errorMessage));

            throw;
        }
    }
}