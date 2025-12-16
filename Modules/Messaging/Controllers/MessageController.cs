using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using YopoBackend.Modules.Messaging.DTOs;
using YopoBackend.Modules.Messaging.Services;

namespace YopoBackend.Modules.Messaging.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost]
        public async Task<ActionResult<MessageResponseDTO>> SendMessage([FromBody] SendMessageDTO messageDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = (User.FindFirst(ClaimTypes.Role)?.Value ?? "User").Trim();

            // Map role to SenderType if needed, or just use the role name
            // The MessageService expects a string for SenderType.
            // Ensure consistency with how ReceiverType is expected (Tenant/User)
            
            // If the role is "PropertyManager" or "FrontDesk" or "SuperAdmin", maybe we want to normalize it to "User" 
            // OR keep it specific. The requirement said "SenderType - 'Tenant' or 'User'".
            // But the DTO validation says "ReceiverType must be 'Tenant' or 'User'".
            // If I send as "PropertyManager", the receiver might not know how to handle it if they expect "User".
            
            // Let's stick to the requirement: "SenderType - 'Tenant' or 'User'"
            // If role is Tenant, SenderType = "Tenant". Else "User".
            
            string senderType = string.Equals(userRole, "Tenant", StringComparison.OrdinalIgnoreCase) ? "Tenant" : "User";

            try
            {
                var result = await _messageService.SendMessageAsync(userId, senderType, messageDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageResponseDTO>>> GetMessages([FromQuery] int? buildingId = null)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = (User.FindFirst(ClaimTypes.Role)?.Value ?? "User").Trim();
            string userType = string.Equals(userRole, "Tenant", StringComparison.OrdinalIgnoreCase) ? "Tenant" : "User";

            var messages = await _messageService.GetMessagesAsync(userId, userType, buildingId);
            return Ok(messages);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MessageResponseDTO>> UpdateMessage(int id, [FromBody] UpdateMessageDTO messageDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = (User.FindFirst(ClaimTypes.Role)?.Value ?? "User").Trim();
            string userType = string.Equals(userRole, "Tenant", StringComparison.OrdinalIgnoreCase) ? "Tenant" : "User";

            try
            {
                var result = await _messageService.UpdateMessageAsync(id, userId, userType, messageDto);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("chat")]
        public async Task<IActionResult> DeleteChat([FromQuery] int participantId, [FromQuery] string participantType)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = (User.FindFirst(ClaimTypes.Role)?.Value ?? "User").Trim();
            string userType = string.Equals(userRole, "Tenant", StringComparison.OrdinalIgnoreCase) ? "Tenant" : "User";

            if (participantId <= 0)
            {
                return BadRequest(new { message = "participantId is required." });
            }

            var normalizedParticipantType = (participantType ?? string.Empty).Trim();
            if (!string.Equals(normalizedParticipantType, "Tenant", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(normalizedParticipantType, "User", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "participantType must be 'Tenant' or 'User'." });
            }

            normalizedParticipantType = char.ToUpper(normalizedParticipantType[0]) + normalizedParticipantType.Substring(1).ToLower();

            try
            {
                var deletedCount = await _messageService.DeleteChatAsync(userId, userType, participantId, normalizedParticipantType);
                return Ok(new { message = "Chat deleted successfully.", deletedCount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkConversationRead([FromQuery] int participantId, [FromQuery] string participantType, [FromQuery] int? buildingId = null)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = (User.FindFirst(ClaimTypes.Role)?.Value ?? "User").Trim();
            string userType = string.Equals(userRole, "Tenant", StringComparison.OrdinalIgnoreCase) ? "Tenant" : "User";

            if (participantId <= 0)
            {
                return BadRequest(new { message = "participantId is required." });
            }

            var normalizedParticipantType = (participantType ?? string.Empty).Trim();
            if (!string.Equals(normalizedParticipantType, "Tenant", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(normalizedParticipantType, "User", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "participantType must be 'Tenant' or 'User'." });
            }

            normalizedParticipantType = char.ToUpper(normalizedParticipantType[0]) + normalizedParticipantType.Substring(1).ToLower();

            var updated = await _messageService.MarkConversationReadAsync(userId, userType, participantId, normalizedParticipantType, buildingId);
            return Ok(new { updated });
        }
    }
}
