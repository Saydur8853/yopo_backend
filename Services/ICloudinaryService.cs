namespace YopoBackend.Services
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadThreadPostImageAsync(byte[] imageBytes, string mimeType);
        Task DeleteImageAsync(string publicId);
    }
}
