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
        /// Gets all invitations asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of invitation response DTOs.</returns>
        Task<IEnumerable<InvitationResponseDTO>> GetAllInvitationsAsync();
        
        /// <summary>
        /// Gets an invitation by its ID asynchronously.
        /// </summary>
        /// <param name="id">The invitation ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the invitation response DTO, or null if not found.</returns>
        Task<InvitationResponseDTO?> GetInvitationByIdAsync(int id);
        
        /// <summary>
        /// Creates a new invitation asynchronously.
        /// </summary>
        /// <param name="createDto">The invitation creation data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created invitation response DTO.</returns>
        Task<InvitationResponseDTO> CreateInvitationAsync(CreateInvitationDTO createDto);
        
        /// <summary>
        /// Updates an existing invitation asynchronously.
        /// </summary>
        /// <param name="id">The invitation ID to update.</param>
        /// <param name="updateDto">The invitation update data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated invitation response DTO, or null if not found.</returns>
        Task<InvitationResponseDTO?> UpdateInvitationAsync(int id, UpdateInvitationDTO updateDto);
        
        /// <summary>
        /// Deletes an invitation asynchronously.
        /// </summary>
        /// <param name="id">The invitation ID to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the deletion was successful.</returns>
        Task<bool> DeleteInvitationAsync(int id);
        
        /// <summary>
        /// Gets all expired invitations asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of expired invitation response DTOs.</returns>
        Task<IEnumerable<InvitationResponseDTO>> GetExpiredInvitationsAsync();
        
        /// <summary>
        /// Gets all active (non-expired) invitations asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of active invitation response DTOs.</returns>
        Task<IEnumerable<InvitationResponseDTO>> GetActiveInvitationsAsync();
        
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
    }
}
