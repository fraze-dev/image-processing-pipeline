using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace ImageProcessingPipeline.Services;

public class ImageProcessor
{
    public async Task<(byte[] processedImage, int width, int height)> ResizeImageAsync(
        Stream imageStream,
        int maxWidth = 800,
        int maxHeight = 800)
    {
        using var image = await Image.LoadAsync(imageStream);

        //Calculate new dimensions maintaing aspect ration
        var ratioX = (double)maxWidth / image.Width;
        var ratioY = (double)maxHeight / image.Height;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = (int)(image.Width * ratio);
        var newHeight = (int)(image.Height * ratio);

        // Resize
        image.Mutate(x => x.Resize(newWidth, newHeight));

        // Convert to byte array
        using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, new JpegEncoder { Quality = 85});

        return (outputStream.ToArray(), newWidth, newHeight);

    }

    public async Task<(byte[] thumbnail, int width, int height)> CreateThumbnailAsync(
        Stream imageStream,
        int size = 200)
    {
        using var image = await Image.LoadAsync(imageStream);

        // Create square thumbnail
        var minDimension = Math.Min(image.Width, image.Height);

        image.Mutate(x => x
            .Resize(new ResizeOptions
            {
                Size = new Size(size, size),
                Mode = ResizeMode.Crop
            }));

        using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, new JpegEncoder { Quality = 75 });

        return (outputStream.ToArray(), size, size);
    }

    public (int width, int height) GetImageDimensions(Stream imageStream)
    {
        var image = Image.Load(imageStream);
        return (image.Width, image.Height);
    }
}