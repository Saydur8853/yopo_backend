using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.ThreadSocial.DTOs
{
    public class CreateThreadPostDTO
    {
        public string? Content { get; set; }

        public string? ImageBase64 { get; set; }

        public int? BuildingId { get; set; }
    }

    public class UpdateThreadPostDTO
    {
        public string? Content { get; set; }

        public string? ImageBase64 { get; set; }
    }

    public class UpdateThreadPostStatusDTO
    {
        public bool IsActive { get; set; }
    }

    public class UpdateThreadPostReactionDTO
    {
        public string? Reaction { get; set; }
    }

    public class ThreadPostResponseDTO
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public string AuthorType { get; set; } = string.Empty;
        public string? AuthorName { get; set; }
        public string? AuthorInfo { get; set; }
        public string? Content { get; set; }
        public string? ImageBase64 { get; set; }
        public string? ImageUrl { get; set; }
        public int? BuildingId { get; set; }
        public bool IsActive { get; set; }
        public int CommentCount { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public string? CurrentUserReaction { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateThreadCommentDTO
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int PostId { get; set; }

        public int? ParentCommentId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateThreadCommentDTO
    {
        [Required]
        public string Content { get; set; } = string.Empty;
    }

    public class ThreadCommentResponseDTO
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorType { get; set; } = string.Empty;
        public string? AuthorName { get; set; }
        public string? AuthorInfo { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
