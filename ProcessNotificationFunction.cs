using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

/*
namespace ImageProcessingPipeline;
public class ProcessNotificationFunction
{
    private readonly ILogger<ProcessNotificationFunction> _logger;

    public ProcessNotificationFunction(ILogger<ProcessNotificationFunction> logger)
    {
        _logger = logger;
    }

    [Function("ProcessNotificationFunction")]
    public void Run(
        [QueueTrigger("image-processing-queue", Connection = "StorageConnectionString")] string queueMessage)
    {
        _logger.LogInformation ($"QUEUE TRIGGER FIRED! Message: {queueMessage}");
    }
    
}
*/

namespace ImageProcessingPipeline;

public class ProcessNotificationFunction
{
    private readonly ILogger<ProcessNotificationFunction> _logger;

    public ProcessNotificationFunction(ILogger<ProcessNotificationFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(ProcessNotificationFunction))]
    public async Task Run(
        [QueueTrigger("image-processing-queue", Connection = "StorageConnectionString")] string queueMessage)
    {
        _logger.LogInformation($"Processing queue message: {queueMessage}");

        try
        {
            // deserialization options
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            //Deserialize the message
            var notification = JsonSerializer.Deserialize<ImageProcessingNotification>(queueMessage, options);

            if (notification == null)
            {
                _logger.LogWarning("Failed to deserialize queue message");
                return;
            }

            _logger.LogInformation($"Notification Status: {notification.Status}");

            //Process depending on status
            if (notification.Status == "Success")
            {
                _logger.LogInformation(
                    $"SUCCESS: Image '{notification.FileName}' processed successfully." +
                    $"Original: {notification.OriginalSize:N0} bytes, " +
                    $"Processed: {notification.ProcessedSize:N0} bytes, " +
                    $"Compression: {CalculateCompressionPercentage(notification.OriginalSize, notification.ProcessedSize):F1}%");
                
                await Task.CompletedTask;
            }
            else if (notification.Status == "Failed")
            {
                _logger.LogError(
                    $"Failed: Image '{notification.FileName}' processing faild. " +
                    $"Error: {notification.Error}");
            }
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError($"JSON deserialization error: {jsonEx.Message}");
            _logger.LogError($"Message content: {queueMessage}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing notification:{ex.Message}");
            _logger.LogError($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private double CalculateCompressionPercentage(long original, long processed)
    {
        if (original == 0) return 0;
        return ((double)(original-processed) / original) * 100;
    }

    //Helper Class
    private class ImageProcessingNotification
    {
        public string FileName {get;set;} = string.Empty;
        public string Status {get;set;} = string.Empty;
        public DateTime ProcessedAt {get;set;}
        public long OriginalSize {get;set;}
        public long ProcessedSize {get;set;}
        public string? Error {get;set;}
    }
}
