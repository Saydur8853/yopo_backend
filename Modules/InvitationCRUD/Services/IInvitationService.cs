using YopoBackend.Modules.InvitationCRUD.DTOs;
using YopoBackend.Modules.InvitationCRUD.Models;

namespace YopoBackend.Modules.InvitationCRUD.Services
{
    /// <summary>
    /// Interface for invitation service operations
    /// </summary>
    public interface IInvitationService
    {
        /// <summary>
        /// Gets all invitations asynchronously with access control.
        /// </summary>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of invitation response DTOs.</returns>
        Task<IEnumerable<InvitationResponseDTO>> GetAllInvitationsAsync(int currentUserId);
        
        /// <summary>
        /// Gets an invitation by its ID asynchronously with access control.
        /// </summary>
        /// <param name="id">The invitation ID.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the invitation response DTO, or null if not found.</returns>
        Task<InvitationResponseDTO?> GetInvitationByIdAsync(int id, int currentUserId);
        
        /// <summary>
        /// Creates a new invitation asynchronously.
        /// </summary>
        /// <param name="createDto">The invitation creation data.</param>
        /// <param name="createdByUserId">The ID of the user creating this invitation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created invitation response DTO.</returns>
        Task<InvitationResponseDTO> CreateInvitationAsync(CreateInvitationDTO createDto, int createdByUserId);
        
        /// <summary>
        /// Updates an existing invitation asynchronously with access control.
        /// </summary>
        /// <param name="id">The invitation ID to update.</param>
        /// <param name="updateDto">The invitation update data.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated invitation response DTO, or null if not found.</returns>
        Task<InvitationResponseDTO?> UpdateInvitationAsync(int id, UpdateInvitationDTO updateDto, int currentUserId);
        
        /// <summary>
        /// Deletes an invitation asynchronously with access control.
        /// </summary>
        /// <param name="id">The invitation ID to delete.</param>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the deletion was successful.</returns>
        Task<bool> DeleteInvitationAsync(int id, int currentUserId);
        
        /// <summary>
        /// Gets all expired invitations asynchronously with access control.
        /// </summary>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of expired invitation response DTOs.</returns>
        Task<IEnumerable<InvitationResponseDTO>> GetExpiredInvitationsAsync(int currentUserId);
        
        /// <summary>
        /// Gets all active (non-expired) invitations asynchronously with access control.
        /// </summary>
        /// <param name="currentUserId">The ID of the current user making the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of active invitation response DTOs.</returns>
        Task<IEnumerable<InvitationResponseDTO>> GetActiveInvitationsAsync(int currentUserId);
        
        /// <summary>
        /// Checks if an invitation exists asynchronously.
        /// </summary>
        /// <param name="id">The invitation ID to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the invitation exists.</returns>
        Task<bool> InvitationExistsAsync(int id);
        
        /// <summary>
        /// Checks if an email address already has an active invitation asynchronously.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the email already has an active invitation.</returns>
        Task<bool> EmailAlreadyInvitedAsync(string email);
        
        /// <summary>
        /// Gets all available user types for invitation dropdown selection.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of available user types.</returns>
        Task<IEnumerable<UserTypeDropdownDTO>> GetAvailableUserTypesAsync();
        
        /// <summary>
        /// Validates if a user type ID exists and is active.
        /// </summary>
        /// <param name="userTypeId">The user type ID to validate.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the user type ID is valid.</returns>
        Task<bool> ValidateUserTypeIdAsync(int userTypeId);
    }
}
