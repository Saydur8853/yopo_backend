namespace YopoBackend.Services
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadThreadPostImageAsync(byte[] imageBytes, string mimeType);
        Task<(string Url, string PublicId)> UploadIdentityDocumentAsync(byte[] fileBytes, string mimeType);
        Task DeleteImageAsync(string publicId);
        Task DeleteAssetAsync(string publicId, string resourceType);
    }
}
