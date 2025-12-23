using YopoBackend.Modules.ThreadSocial.DTOs;

namespace YopoBackend.Modules.ThreadSocial.Services
{
    public interface IThreadSocialService
    {
        Task<ThreadPostResponseDTO> CreatePostAsync(int userId, string userRole, CreateThreadPostDTO dto);
        Task<(List<ThreadPostResponseDTO> Posts, int TotalRecords)> GetPostsAsync(int userId, string userRole, int? buildingId, int? authorId, int page, int pageSize);
        Task<ThreadPostResponseDTO> UpdatePostAsync(int postId, int userId, string userRole, UpdateThreadPostDTO dto);
        Task<ThreadPostResponseDTO> UpdatePostStatusAsync(int postId, int userId, string userRole, bool isActive);
        Task DeletePostAsync(int postId, int userId, string userRole);
        Task<ThreadCommentResponseDTO> CreateCommentAsync(int userId, string userRole, CreateThreadCommentDTO dto);
        Task<(List<ThreadCommentResponseDTO> Comments, int TotalRecords)> GetCommentsAsync(int userId, string userRole, int postId, int? authorId, int page, int pageSize);
        Task<ThreadCommentResponseDTO> UpdateCommentAsync(int commentId, int userId, string userRole, UpdateThreadCommentDTO dto);
        Task DeleteCommentAsync(int commentId, int userId, string userRole);
    }
}
