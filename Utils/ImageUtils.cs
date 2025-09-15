using System.Drawing;
using System.Drawing.Imaging;

namespace YopoBackend.Utils
{
    /// <summary>
    /// Utility class for image processing and validation.
    /// </summary>
    public static class ImageUtils
    {
        /// <summary>
        /// Maximum allowed image size in bytes (5MB).
        /// </summary>
        public const int MAX_IMAGE_SIZE_BYTES = 5 * 1024 * 1024; // 5MB

        /// <summary>
        /// Maximum allowed image dimensions (width or height).
        /// </summary>
        public const int MAX_IMAGE_DIMENSION = 2048; // 2048px

        /// <summary>
        /// Supported image MIME types.
        /// </summary>
        public static readonly HashSet<string> SupportedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/jpg", 
            "image/png",
            "image/gif",
            "image/bmp",
            "image/webp"
        };

        /// <summary>
        /// Validates a base64 encoded image string.
        /// </summary>
        /// <param name="base64Image">The base64 encoded image string.</param>
        /// <returns>A validation result with success status and error message.</returns>
        public static ImageValidationResult ValidateBase64Image(string base64Image)
        {
            if (string.IsNullOrWhiteSpace(base64Image))
            {
                return new ImageValidationResult { IsValid = false, ErrorMessage = "Image data is required." };
            }

            try
            {
                // Remove data URL prefix if present (data:image/jpeg;base64,...)
                var base64Data = base64Image;
                if (base64Image.Contains(","))
                {
                    var parts = base64Image.Split(',');
                    if (parts.Length == 2)
                    {
                        base64Data = parts[1];
                    }
                }

                // Validate base64 format
                byte[] imageBytes;
                try
                {
                    imageBytes = Convert.FromBase64String(base64Data);
                }
                catch (FormatException)
                {
                    return new ImageValidationResult { IsValid = false, ErrorMessage = "Invalid base64 image format." };
                }

                // Check file size
                if (imageBytes.Length > MAX_IMAGE_SIZE_BYTES)
                {
                    var sizeMB = imageBytes.Length / (1024.0 * 1024.0);
                    return new ImageValidationResult 
                    { 
                        IsValid = false, 
                        ErrorMessage = $"Image size ({sizeMB:F1}MB) exceeds maximum allowed size (5MB)." 
                    };
                }

                // Validate image format and get MIME type
                var mimeType = GetImageMimeType(imageBytes);
                if (string.IsNullOrEmpty(mimeType) || !SupportedMimeTypes.Contains(mimeType))
                {
                    return new ImageValidationResult 
                    { 
                        IsValid = false, 
                        ErrorMessage = "Unsupported image format. Supported formats: JPEG, PNG, GIF, BMP, WebP." 
                    };
                }

                // Validate image dimensions
                try
                {
                    using var stream = new MemoryStream(imageBytes);
                    using var image = Image.FromStream(stream);
                    
                    if (image.Width > MAX_IMAGE_DIMENSION || image.Height > MAX_IMAGE_DIMENSION)
                    {
                        return new ImageValidationResult 
                        { 
                            IsValid = false, 
                            ErrorMessage = $"Image dimensions ({image.Width}x{image.Height}) exceed maximum allowed size ({MAX_IMAGE_DIMENSION}x{MAX_IMAGE_DIMENSION})." 
                        };
                    }
                }
                catch (ArgumentException)
                {
                    return new ImageValidationResult { IsValid = false, ErrorMessage = "Invalid or corrupted image file." };
                }

                return new ImageValidationResult 
                { 
                    IsValid = true, 
                    ImageBytes = imageBytes, 
                    MimeType = mimeType 
                };
            }
            catch (Exception ex)
            {
                return new ImageValidationResult { IsValid = false, ErrorMessage = $"Image validation error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Converts binary image data to base64 encoded string with data URL prefix.
        /// </summary>
        /// <param name="imageBytes">The binary image data.</param>
        /// <param name="mimeType">The MIME type of the image.</param>
        /// <returns>Base64 encoded string with data URL prefix.</returns>
        public static string? ConvertToBase64DataUrl(byte[]? imageBytes, string? mimeType)
        {
            if (imageBytes == null || imageBytes.Length == 0 || string.IsNullOrEmpty(mimeType))
            {
                return null;
            }

            var base64 = Convert.ToBase64String(imageBytes);
            return $"data:{mimeType};base64,{base64}";
        }

        /// <summary>
        /// Converts binary image data to base64 encoded string without data URL prefix.
        /// </summary>
        /// <param name="imageBytes">The binary image data.</param>
        /// <returns>Base64 encoded string.</returns>
        public static string? ConvertToBase64(byte[]? imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
            {
                return null;
            }

            return Convert.ToBase64String(imageBytes);
        }

        /// <summary>
        /// Gets the MIME type of an image from its binary data.
        /// </summary>
        /// <param name="imageBytes">The binary image data.</param>
        /// <returns>The MIME type or null if not recognized.</returns>
        public static string? GetImageMimeType(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length < 8)
            {
                return null;
            }

            // Check file signatures (magic numbers)
            
            // JPEG: FF D8 FF
            if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF)
            {
                return "image/jpeg";
            }

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47 &&
                imageBytes[4] == 0x0D && imageBytes[5] == 0x0A && imageBytes[6] == 0x1A && imageBytes[7] == 0x0A)
            {
                return "image/png";
            }

            // GIF: GIF87a or GIF89a
            if (imageBytes.Length >= 6)
            {
                var gifHeader = System.Text.Encoding.ASCII.GetString(imageBytes, 0, 6);
                if (gifHeader == "GIF87a" || gifHeader == "GIF89a")
                {
                    return "image/gif";
                }
            }

            // BMP: BM
            if (imageBytes[0] == 0x42 && imageBytes[1] == 0x4D)
            {
                return "image/bmp";
            }

            // WebP: RIFF....WEBP
            if (imageBytes.Length >= 12 && 
                imageBytes[0] == 0x52 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46 && imageBytes[3] == 0x46 &&
                imageBytes[8] == 0x57 && imageBytes[9] == 0x45 && imageBytes[10] == 0x42 && imageBytes[11] == 0x50)
            {
                return "image/webp";
            }

            return null;
        }

        /// <summary>
        /// Resizes an image to fit within the specified maximum dimensions while maintaining aspect ratio.
        /// </summary>
        /// <param name="imageBytes">The original image bytes.</param>
        /// <param name="maxWidth">Maximum width.</param>
        /// <param name="maxHeight">Maximum height.</param>
        /// <param name="quality">JPEG quality (1-100). Only applies to JPEG images.</param>
        /// <returns>Resized image bytes in JPEG format.</returns>
        public static byte[] ResizeImage(byte[] imageBytes, int maxWidth, int maxHeight, int quality = 85)
        {
            using var stream = new MemoryStream(imageBytes);
            using var originalImage = Image.FromStream(stream);

            // Calculate new dimensions
            var ratioX = (double)maxWidth / originalImage.Width;
            var ratioY = (double)maxHeight / originalImage.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(originalImage.Width * ratio);
            var newHeight = (int)(originalImage.Height * ratio);

            // Create resized image
            using var resizedImage = new Bitmap(newWidth, newHeight);
            using var graphics = Graphics.FromImage(resizedImage);
            
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            
            graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);

            // Save to memory stream as JPEG
            using var outputStream = new MemoryStream();
            var jpegEncoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            
            resizedImage.Save(outputStream, jpegEncoder, encoderParameters);
            
            return outputStream.ToArray();
        }

        /// <summary>
        /// Gets the image encoder for the specified format.
        /// </summary>
        /// <param name="format">The image format.</param>
        /// <returns>The image codec info.</returns>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            throw new ArgumentException("No encoder found for format: " + format);
        }
    }

    /// <summary>
    /// Result of image validation.
    /// </summary>
    public class ImageValidationResult
    {
        /// <summary>
        /// Gets or sets whether the image is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the error message if validation failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the validated image bytes.
        /// </summary>
        public byte[]? ImageBytes { get; set; }

        /// <summary>
        /// Gets or sets the detected MIME type.
        /// </summary>
        public string? MimeType { get; set; }
    }
}