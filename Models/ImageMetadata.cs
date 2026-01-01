using Azure;
using Azure.Data.Tables;

namespace ImageProcessingPipeline.Models;

public class ImageMetadata : ITableEntity
{
    public string PartitionKey {get;set;} = "images";
    public string RowKey {get;set;} = Guid.NewGuid().ToString();
    public DateTimeOffset? Timestamp {get;set;}
    public ETag ETag {get;set;}

    //Custom properties
    public string OriginalFileName {get;set;} = string.Empty;
    public string ProcessedFileName {get;set;} = string.Empty;
    public string ThumbnailFileName {get;set;} = string.Empty;
    public long OriginalSizeBytes {get;set;}
    public long ProcessedSizeBytes {get;set;}
    public int OriginalWidth {get;set;}
    public int OriginalHeight {get;set;}
    public int ProcessedWidth {get;set;}
    public int ProcessedHeight {get;set;}
    public string ProcessingStatus {get;set;} = "Processing";
    public DateTime UploadedAt {get;set;}
    public DateTime? ProcessedAt {get;set;}
    
}