using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace YopoBackend.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly string _folder;

        public CloudinaryService(IConfiguration configuration)
        {
            var account = BuildAccount(configuration);
            _cloudinary = new Cloudinary(account)
            {
                Api = { Secure = true }
            };
            _folder = configuration["Cloudinary:Folder"] ?? "thread_posts";
        }

        public async Task<(string Url, string PublicId)> UploadThreadPostImageAsync(byte[] imageBytes, string mimeType)
        {
            if (imageBytes.Length == 0)
            {
                throw new ArgumentException("Image data is empty.");
            }

            var fileExtension = GetExtensionFromMimeType(mimeType);
            var fileName = $"thread-post-{Guid.NewGuid():N}{fileExtension}";

            await using var stream = new MemoryStream(imageBytes);
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                Folder = _folder
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            if (result.Error != null)
            {
                throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");
            }

            if (result.StatusCode != HttpStatusCode.OK && result.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException("Cloudinary upload failed.");
            }

            var url = result.SecureUrl?.ToString() ?? result.Url?.ToString();
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(result.PublicId))
            {
                throw new InvalidOperationException("Cloudinary upload returned an invalid response.");
            }

            return (url, result.PublicId);
        }

        public async Task DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return;
            }

            var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
            if (result.Error != null)
            {
                throw new InvalidOperationException($"Cloudinary delete failed: {result.Error.Message}");
            }
        }

        private static Account BuildAccount(IConfiguration configuration)
        {
            var cloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL")
                ?? configuration["Cloudinary:Url"];
            if (!string.IsNullOrWhiteSpace(cloudinaryUrl))
            {
                var url = cloudinaryUrl.Contains("://", StringComparison.Ordinal)
                    ? cloudinaryUrl
                    : $"cloudinary://{cloudinaryUrl}";
                var uri = new Uri(url);
                var cloudName = uri.Host;
                var userInfo = uri.UserInfo;
                var parts = userInfo.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && !string.IsNullOrWhiteSpace(cloudName))
                {
                    return new Account(cloudName, parts[0], parts[1]);
                }
            }

            var envCloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
            var envApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
            var envApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");

            if (!string.IsNullOrWhiteSpace(envCloudName) &&
                !string.IsNullOrWhiteSpace(envApiKey) &&
                !string.IsNullOrWhiteSpace(envApiSecret))
            {
                return new Account(envCloudName, envApiKey, envApiSecret);
            }

            var fallbackCloudName = configuration["Cloudinary:CloudName"];
            var fallbackApiKey = configuration["Cloudinary:ApiKey"];
            var fallbackApiSecret = configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrWhiteSpace(fallbackCloudName) ||
                string.IsNullOrWhiteSpace(fallbackApiKey) ||
                string.IsNullOrWhiteSpace(fallbackApiSecret))
            {
                throw new InvalidOperationException("Cloudinary configuration is missing.");
            }

            return new Account(fallbackCloudName, fallbackApiKey, fallbackApiSecret);
        }

        private static string GetExtensionFromMimeType(string mimeType)
        {
            return mimeType switch
            {
                "image/jpeg" => ".jpg",
                "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                "image/bmp" => ".bmp",
                _ => ".img"
            };
        }
    }
}
