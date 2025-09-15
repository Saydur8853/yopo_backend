using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.UserCRUD.DTOs
{
    /// <summary>
    /// Data Transfer Object for updating user profile photo.
    /// </summary>
    public class UpdateProfilePhotoRequestDTO
    {
        /// <summary>
        /// Base64 encoded profile photo image.
        /// Set to empty string to remove the photo, null or missing to leave unchanged.
        /// </summary>
        /// <example>data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD...</example>
        public string? ProfilePhotoBase64 { get; set; }
    }
}