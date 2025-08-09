using Microsoft.AspNetCore.Mvc;
using YopoBackend.Modules.InvitationCRUD.DTOs;
using YopoBackend.Modules.InvitationCRUD.Services;
using YopoBackend.Constants;

namespace YopoBackend.Modules.InvitationCRUD.Controllers
{
    /// <summary>
    /// Controller for managing invitations (Module ID: {ModuleConstants.INVITATION_MODULE_ID})
    /// This controller belongs to the InvitationCRUD module.
    /// </summary>
    [ApiController]
    [Route("api/invitations")]
    [Produces("application/json")]
    [Tags("02-Invitations")]
    public class InvitationsController : ControllerBase
    {
        private readonly IInvitationService _invitationService;
        private readonly ILogger<InvitationsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvitationsController"/> class.
        /// </summary>
        /// <param name="invitationService">The invitation service.</param>
        /// <param name="logger">The logger instance.</param>
        public InvitationsController(IInvitationService invitationService, ILogger<InvitationsController> logger)
        {
            _invitationService = invitationService;
            _logger = logger;
        }

        /// <summary>
        /// Get all invitations
        /// </summary>
        /// <returns>List of all invitations</returns>
        /// <response code="200">Returns the list of invitations</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InvitationResponseDTO>>> GetAllInvitations()
        {
            var invitations = await _invitationService.GetAllInvitationsAsync();
            return Ok(invitations);
        }

        /// <summary>
        /// Get a specific invitation by ID
        /// </summary>
        /// <param name="id">Invitation ID</param>
        /// <returns>Invitation details</returns>
        /// <response code="200">Returns the invitation</response>
        /// <response code="404">If the invitation is not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InvitationResponseDTO>> GetInvitation(int id)
        {
            var invitation = await _invitationService.GetInvitationByIdAsync(id);
            if (invitation == null)
            {
                return NotFound($"Invitation with ID {id} not found.");
            }

            return Ok(invitation);
        }

        /// <summary>
        /// Create a new invitation
        /// </summary>
        /// <param name="createDto">Invitation data</param>
        /// <returns>Created invitation</returns>
        /// <response code="201">Returns the newly created invitation</response>
        /// <response code="400">If the invitation data is invalid</response>
        /// <response code="409">If email is already invited and active</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<InvitationResponseDTO>> CreateInvitation([FromBody] CreateInvitationDTO createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if email is already invited and active
            var emailAlreadyInvited = await _invitationService.EmailAlreadyInvitedAsync(createDto.EmailAddress);
            if (emailAlreadyInvited)
            {
                return Conflict($"Email {createDto.EmailAddress} already has an active invitation.");
            }

            // Validate user type ID
            var isValidUserType = await _invitationService.ValidateUserTypeIdAsync(createDto.UserTypeId);
            if (!isValidUserType)
            {
                return BadRequest($"User type with ID {createDto.UserTypeId} is not valid or not active.");
            }

            var invitation = await _invitationService.CreateInvitationAsync(createDto);
            
            return CreatedAtAction(
                nameof(GetInvitation), 
                new { id = invitation.Id }, 
                invitation);
        }

        /// <summary>
        /// Update an existing invitation
        /// </summary>
        /// <param name="id">Invitation ID</param>
        /// <param name="updateDto">Updated invitation data</param>
        /// <returns>Updated invitation</returns>
        /// <response code="200">Returns the updated invitation</response>
        /// <response code="400">If the update data is invalid</response>
        /// <response code="404">If the invitation is not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InvitationResponseDTO>> UpdateInvitation(int id, [FromBody] UpdateInvitationDTO updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate user type ID if provided
            if (updateDto.UserTypeId.HasValue)
            {
                var isValidUserType = await _invitationService.ValidateUserTypeIdAsync(updateDto.UserTypeId.Value);
                if (!isValidUserType)
                {
                    return BadRequest($"User type with ID {updateDto.UserTypeId.Value} is not valid or not active.");
                }
            }

            var invitation = await _invitationService.UpdateInvitationAsync(id, updateDto);
            if (invitation == null)
            {
                return NotFound($"Invitation with ID {id} not found.");
            }

            return Ok(invitation);
        }

        /// <summary>
        /// Delete an invitation
        /// </summary>
        /// <param name="id">Invitation ID</param>
        /// <returns>No content</returns>
        /// <response code="204">Invitation successfully deleted</response>
        /// <response code="404">If the invitation is not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteInvitation(int id)
        {
            var deleted = await _invitationService.DeleteInvitationAsync(id);
            if (!deleted)
            {
                return NotFound($"Invitation with ID {id} not found.");
            }

            return NoContent();
        }

        /// <summary>
        /// Get all expired invitations
        /// </summary>
        /// <returns>List of expired invitations</returns>
        /// <response code="200">Returns the list of expired invitations</response>
        [HttpGet("expired")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InvitationResponseDTO>>> GetExpiredInvitations()
        {
            var expiredInvitations = await _invitationService.GetExpiredInvitationsAsync();
            return Ok(expiredInvitations);
        }

        /// <summary>
        /// Get all active (non-expired) invitations
        /// </summary>
        /// <returns>List of active invitations</returns>
        /// <response code="200">Returns the list of active invitations</response>
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InvitationResponseDTO>>> GetActiveInvitations()
        {
            var activeInvitations = await _invitationService.GetActiveInvitationsAsync();
            return Ok(activeInvitations);
        }

        /// <summary>
        /// Check if an email already has an active invitation
        /// </summary>
        /// <param name="email">Email address to check</param>
        /// <returns>Boolean indicating if email is already invited</returns>
        /// <response code="200">Returns the check result</response>
        /// <response code="400">If email format is invalid</response>
        [HttpGet("check-email/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> CheckEmailInvitation(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                return BadRequest("Invalid email format.");
            }

            var exists = await _invitationService.EmailAlreadyInvitedAsync(email);
            return Ok(new { emailAlreadyInvited = exists });
        }

        /// <summary>
        /// Get available user types for invitation dropdown
        /// </summary>
        /// <returns>List of available user types</returns>
        /// <response code="200">Returns the list of available user types</response>
        [HttpGet("user-types")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserTypeDropdownDTO>>> GetAvailableUserTypes()
        {
            var userTypes = await _invitationService.GetAvailableUserTypesAsync();
            return Ok(userTypes);
        }
    }
}
