using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using YopoBackend.Data;
using YopoBackend.Hubs;
using YopoBackend.Modules.ThreadSocial.DTOs;
using YopoBackend.Modules.ThreadSocial.Models;
using YopoBackend.Services;
using YopoBackend.Constants;
using YopoBackend.Utils;

namespace YopoBackend.Modules.ThreadSocial.Services
{
    public class ThreadSocialService : BaseAccessControlService, IThreadSocialService
    {
        private readonly IHubContext<ThreadSocialHub> _hubContext;

        public ThreadSocialService(ApplicationDbContext context, IHubContext<ThreadSocialHub> hubContext) : base(context)
        {
            _hubContext = hubContext;
        }

        public async Task<ThreadPostResponseDTO> CreatePostAsync(int userId, string userRole, CreateThreadPostDTO dto)
        {
            EnsureTenantOrManager(userRole);

            var trimmedContent = NormalizeContent(dto.Content);
            var hasImage = !string.IsNullOrWhiteSpace(dto.ImageBase64);

            if (trimmedContent == null && !hasImage)
            {
                throw new ArgumentException("Post must include text or an image.");
            }

            byte[]? imageBytes = null;
            string? imageMimeType = null;
            if (hasImage)
            {
                var validation = ImageUtils.ValidateBase64Image(dto.ImageBase64!);
                if (!validation.IsValid)
                {
                    throw new ArgumentException($"Invalid image: {validation.ErrorMessage}");
                }

                imageBytes = validation.ImageBytes;
                imageMimeType = validation.MimeType;
            }

            var buildingId = await ResolveAccessibleBuildingIdAsync(userId, userRole, dto.BuildingId);
            if (!buildingId.HasValue)
            {
                if (IsTenantRole(userRole))
                {
                    throw new InvalidOperationException("Tenant is not assigned to a building.");
                }
                throw new InvalidOperationException("Building selection is required.");
            }

            var post = new ThreadPost
            {
                AuthorId = userId,
                AuthorType = GetAuthorTypeForRole(userRole),
                Content = trimmedContent,
                Image = imageBytes,
                ImageMimeType = imageMimeType,
                BuildingId = buildingId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ThreadPosts.Add(post);
            await _context.SaveChangesAsync();

            var response = await BuildPostResponseAsync(post, 0);

            if (buildingId.HasValue)
            {
                await _hubContext.Clients
                    .Group(ThreadSocialHub.GroupName(buildingId.Value))
                    .SendAsync("ThreadPostCreated", response);
            }

            return response;
        }

        public async Task<(List<ThreadPostResponseDTO> Posts, int TotalRecords)> GetPostsAsync(int userId, string userRole, int? buildingId, int? authorId, int page, int pageSize)
        {
            EnsureTenantOrManager(userRole);

            var effectiveBuildingId = await ResolveAccessibleBuildingIdAsync(userId, userRole, buildingId);
            if (!effectiveBuildingId.HasValue) return (new List<ThreadPostResponseDTO>(), 0);

            var query = _context.ThreadPosts
                .AsNoTracking()
                .Where(p => p.BuildingId == effectiveBuildingId.Value);

            if (authorId.HasValue && authorId.Value > 0)
            {
                query = query.Where(p => p.AuthorId == authorId.Value);
            }

            var totalRecords = await query.CountAsync();

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (posts.Count == 0)
            {
                return (new List<ThreadPostResponseDTO>(), totalRecords);
            }

            var postIds = posts.Select(p => p.Id).ToList();
            var authorIds = posts.Select(p => p.AuthorId).Distinct().ToList();

            var nameLookup = await BuildUserNameLookupAsync(authorIds);
            var commentCounts = await BuildCommentCountsAsync(postIds);

            var response = posts
                .Select(p => MapPostToResponse(p, nameLookup, commentCounts))
                .ToList();

            return (response, totalRecords);
        }

        public async Task<ThreadPostResponseDTO> UpdatePostAsync(int postId, int userId, string userRole, UpdateThreadPostDTO dto)
        {
            EnsureTenantOrManager(userRole);

            var post = await GetAccessiblePostAsync(postId, userId, userRole);

            var authorType = GetAuthorTypeForRole(userRole);
            if (post.AuthorId != userId || !string.Equals(post.AuthorType, authorType, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("You can only edit your own posts.");
            }

            if (dto.Content != null)
            {
                post.Content = NormalizeContent(dto.Content);
            }

            if (dto.ImageBase64 != null)
            {
                if (string.IsNullOrWhiteSpace(dto.ImageBase64))
                {
                    post.Image = null;
                    post.ImageMimeType = null;
                }
                else
                {
                    var validation = ImageUtils.ValidateBase64Image(dto.ImageBase64);
                    if (!validation.IsValid)
                    {
                        throw new ArgumentException($"Invalid image: {validation.ErrorMessage}");
                    }

                    post.Image = validation.ImageBytes;
                    post.ImageMimeType = validation.MimeType;
                }
            }

            if (post.Content == null && post.Image == null)
            {
                throw new ArgumentException("Post must include text or an image.");
            }

            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var commentCounts = await BuildCommentCountsAsync(new List<int> { post.Id });
            var response = await BuildPostResponseAsync(post, commentCounts.TryGetValue(post.Id, out var count) ? count : 0);

            if (post.BuildingId.HasValue)
            {
                await _hubContext.Clients
                    .Group(ThreadSocialHub.GroupName(post.BuildingId.Value))
                    .SendAsync("ThreadPostUpdated", response);
            }

            return response;
        }

        public async Task DeletePostAsync(int postId, int userId, string userRole)
        {
            EnsureTenantOrManager(userRole);

            var post = await GetAccessiblePostAsync(postId, userId, userRole);

            var authorType = GetAuthorTypeForRole(userRole);
            if (post.AuthorId != userId || !string.Equals(post.AuthorType, authorType, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("You can only delete your own posts.");
            }

            var buildingId = post.BuildingId;

            _context.ThreadPosts.Remove(post);
            await _context.SaveChangesAsync();

            if (buildingId.HasValue)
            {
                await _hubContext.Clients
                    .Group(ThreadSocialHub.GroupName(buildingId.Value))
                    .SendAsync("ThreadPostDeleted", new { postId = post.Id });
            }
        }

        public async Task<ThreadCommentResponseDTO> CreateCommentAsync(int userId, string userRole, CreateThreadCommentDTO dto)
        {
            EnsureTenantOrManager(userRole);

            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                throw new ArgumentException("Comment content is required.");
            }

            if (dto.PostId <= 0)
            {
                throw new ArgumentException("PostId is required.");
            }

            var post = await GetAccessiblePostAsync(dto.PostId, userId, userRole);

            if (dto.ParentCommentId.HasValue)
            {
                var parent = await _context.ThreadComments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == dto.ParentCommentId.Value);
                if (parent == null || parent.PostId != dto.PostId)
                {
                    throw new ArgumentException("Invalid parent comment.");
                }
            }
            var authorType = GetAuthorTypeForRole(userRole);

            var comment = new ThreadComment
            {
                PostId = post.Id,
                ParentCommentId = dto.ParentCommentId,
                AuthorId = userId,
                AuthorType = authorType,
                Content = dto.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.ThreadComments.Add(comment);
            await _context.SaveChangesAsync();

            var response = await BuildCommentResponseAsync(comment);

            if (post.BuildingId.HasValue)
            {
                await _hubContext.Clients
                    .Group(ThreadSocialHub.GroupName(post.BuildingId.Value))
                    .SendAsync("ThreadCommentCreated", response);
            }

            return response;
        }

        public async Task<(List<ThreadCommentResponseDTO> Comments, int TotalRecords)> GetCommentsAsync(int userId, string userRole, int postId, int? authorId, int page, int pageSize)
        {
            EnsureTenantOrManager(userRole);

            if (postId <= 0)
            {
                throw new ArgumentException("postId is required.");
            }

            await GetAccessiblePostAsync(postId, userId, userRole);

            var query = _context.ThreadComments
                .AsNoTracking()
                .Where(c => c.PostId == postId);

            if (authorId.HasValue && authorId.Value > 0)
            {
                query = query.Where(c => c.AuthorId == authorId.Value);
            }

            var totalRecords = await query.CountAsync();

            var comments = await query
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (comments.Count == 0)
            {
                return (new List<ThreadCommentResponseDTO>(), totalRecords);
            }

            var authorIds = comments.Select(c => c.AuthorId).Distinct().ToList();
            var nameLookup = await BuildUserNameLookupAsync(authorIds);

            var response = comments
                .Select(c => MapCommentToResponse(c, nameLookup))
                .ToList();

            return (response, totalRecords);
        }

        public async Task<ThreadCommentResponseDTO> UpdateCommentAsync(int commentId, int userId, string userRole, UpdateThreadCommentDTO dto)
        {
            EnsureTenantOrManager(userRole);

            var comment = await _context.ThreadComments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
            {
                throw new KeyNotFoundException("Comment not found.");
            }

            var authorType = GetAuthorTypeForRole(userRole);
            if (comment.AuthorId != userId || !string.Equals(comment.AuthorType, authorType, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("You can only edit your own comments.");
            }

            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                throw new ArgumentException("Comment content is required.");
            }

            comment.Content = dto.Content.Trim();
            comment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var response = await BuildCommentResponseAsync(comment);

            var post = await GetAccessiblePostAsync(comment.PostId, userId, userRole);
            if (post.BuildingId.HasValue)
            {
                await _hubContext.Clients
                    .Group(ThreadSocialHub.GroupName(post.BuildingId.Value))
                    .SendAsync("ThreadCommentUpdated", response);
            }

            return response;
        }

        public async Task DeleteCommentAsync(int commentId, int userId, string userRole)
        {
            EnsureTenantOrManager(userRole);

            var comment = await _context.ThreadComments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
            {
                throw new KeyNotFoundException("Comment not found.");
            }

            var authorType = GetAuthorTypeForRole(userRole);
            if (comment.AuthorId != userId || !string.Equals(comment.AuthorType, authorType, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("You can only delete your own comments.");
            }

            var post = await GetAccessiblePostAsync(comment.PostId, userId, userRole);
            var buildingId = post.BuildingId;

            _context.ThreadComments.Remove(comment);
            await _context.SaveChangesAsync();

            if (buildingId.HasValue)
            {
                await _hubContext.Clients
                    .Group(ThreadSocialHub.GroupName(buildingId.Value))
                    .SendAsync("ThreadCommentDeleted", new { commentId = comment.Id, postId = comment.PostId });
            }
        }

        private static void EnsureTenant(string userRole)
        {
            if (!string.Equals(userRole, "Tenant", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Only tenants can access this resource.");
            }
        }

        private static void EnsureTenantOrManager(string userRole)
        {
            if (!IsTenantRole(userRole) && !IsPropertyManagerRole(userRole))
            {
                throw new UnauthorizedAccessException("Only tenants or property managers can access this resource.");
            }
        }

        private static bool IsTenantRole(string userRole)
        {
            return string.Equals(userRole, UserTypeConstants.TENANT_USER_TYPE_NAME, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPropertyManagerRole(string userRole)
        {
            return string.Equals(userRole, UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_NAME, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetAuthorTypeForRole(string userRole)
        {
            return IsTenantRole(userRole) ? "Tenant" : "User";
        }

        private async Task<ThreadPost> GetAccessiblePostAsync(int postId, int userId, string userRole)
        {
            var post = await _context.ThreadPosts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
            {
                throw new KeyNotFoundException("Post not found.");
            }

            if (post.BuildingId.HasValue)
            {
                await ResolveAccessibleBuildingIdAsync(userId, userRole, post.BuildingId.Value);
            }

            return post;
        }

        private static string? NormalizeContent(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            return content.Trim();
        }

        private async Task<ThreadPostResponseDTO> BuildPostResponseAsync(ThreadPost post, int commentCount)
        {
            var nameLookup = await BuildUserNameLookupAsync(new List<int> { post.AuthorId });
            return MapPostToResponse(post, nameLookup, new Dictionary<int, int> { { post.Id, commentCount } });
        }

        private async Task<ThreadCommentResponseDTO> BuildCommentResponseAsync(ThreadComment comment)
        {
            var nameLookup = await BuildUserNameLookupAsync(new List<int> { comment.AuthorId });
            return MapCommentToResponse(comment, nameLookup);
        }

        private ThreadPostResponseDTO MapPostToResponse(ThreadPost post, IDictionary<int, string> nameLookup, IDictionary<int, int> commentCounts)
        {
            var authorName = nameLookup.TryGetValue(post.AuthorId, out var name) ? name : null;
            var commentCount = commentCounts.TryGetValue(post.Id, out var count) ? count : 0;

            return new ThreadPostResponseDTO
            {
                Id = post.Id,
                AuthorId = post.AuthorId,
                AuthorType = post.AuthorType,
                AuthorName = authorName,
                Content = post.Content,
                ImageBase64 = ImageUtils.ConvertToBase64DataUrl(post.Image, post.ImageMimeType),
                BuildingId = post.BuildingId,
                CommentCount = commentCount,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt
            };
        }

        private ThreadCommentResponseDTO MapCommentToResponse(ThreadComment comment, IDictionary<int, string> nameLookup)
        {
            var authorName = nameLookup.TryGetValue(comment.AuthorId, out var name) ? name : null;

            return new ThreadCommentResponseDTO
            {
                Id = comment.Id,
                PostId = comment.PostId,
                ParentCommentId = comment.ParentCommentId,
                AuthorId = comment.AuthorId,
                AuthorType = comment.AuthorType,
                AuthorName = authorName,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }

        private async Task<Dictionary<int, string>> BuildUserNameLookupAsync(IEnumerable<int> userIds)
        {
            var ids = userIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return new Dictionary<int, string>();
            }

            var users = await _context.Users
                .AsNoTracking()
                .Where(u => ids.Contains(u.Id))
                .Select(u => new { u.Id, u.Name })
                .ToListAsync();

            return users.ToDictionary(u => u.Id, u => u.Name ?? string.Empty);
        }

        private async Task<Dictionary<int, int>> BuildCommentCountsAsync(IEnumerable<int> postIds)
        {
            var ids = postIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return new Dictionary<int, int>();
            }

            var counts = await _context.ThreadComments
                .AsNoTracking()
                .Where(c => ids.Contains(c.PostId))
                .GroupBy(c => c.PostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToListAsync();

            return counts.ToDictionary(c => c.PostId, c => c.Count);
        }

        private async Task<int?> ResolveTenantBuildingIdAsync(int tenantUserId)
        {
            var unit = await _context.Units
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.TenantId == tenantUserId);
            if (unit != null)
            {
                return unit.BuildingId;
            }

            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TenantId == tenantUserId);
            if (tenant != null)
            {
                return tenant.BuildingId;
            }

            return null;
        }

        private async Task<int?> ResolveAccessibleBuildingIdAsync(int userId, string userRole, int? buildingId)
        {
            if (IsTenantRole(userRole))
            {
                var tenantBuildingId = await ResolveTenantBuildingIdAsync(userId);
                if (!tenantBuildingId.HasValue)
                {
                    return null;
                }

                if (buildingId.HasValue && buildingId.Value != tenantBuildingId.Value)
                {
                    throw new UnauthorizedAccessException("You do not have access to this building.");
                }

                return tenantBuildingId.Value;
            }

            if (!IsPropertyManagerRole(userRole))
            {
                throw new UnauthorizedAccessException("Only tenants or property managers can access this resource.");
            }

            if (!buildingId.HasValue)
            {
                return null;
            }

            var hasAccess = await HasBuildingAccessAsync(userId, buildingId.Value);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("You do not have access to this building.");
            }

            return buildingId.Value;
        }

        private async Task<bool> HasBuildingAccessAsync(int userId, int buildingId)
        {
            var currentUser = await GetUserWithAccessControlAsync(userId);
            if (currentUser == null)
            {
                return false;
            }

            var query = _context.Buildings.AsQueryable();
            query = await ApplyAccessControlAsync(query, userId);

            if (currentUser.UserTypeId != UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID)
            {
                var explicitBuildingIds = await _context.UserBuildingPermissions
                    .Where(p => p.UserId == userId && p.IsActive)
                    .Select(p => p.BuildingId)
                    .ToListAsync();

                if (explicitBuildingIds.Any())
                {
                    query = query.Where(b => explicitBuildingIds.Contains(b.BuildingId));
                }
                else if (currentUser.UserTypeId != UserTypeConstants.PROPERTY_MANAGER_USER_TYPE_ID && currentUser.InviteById.HasValue)
                {
                    return false;
                }
            }

            return await query.AnyAsync(b => b.BuildingId == buildingId);
        }
    }
}
