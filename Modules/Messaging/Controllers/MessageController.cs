using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            // Map role to SenderType if needed, or just use the role name
            // The MessageService expects a string for SenderType.
            // Ensure consistency with how ReceiverType is expected (Tenant/User)
            
            // If the role is "PropertyManager" or "FrontDesk" or "SuperAdmin", maybe we want to normalize it to "User" 
            // OR keep it specific. The requirement said "SenderType - 'Tenant' or 'User'".
            // But the DTO validation says "ReceiverType must be 'Tenant' or 'User'".
            // If I send as "PropertyManager", the receiver might not know how to handle it if they expect "User".
            
            // Let's stick to the requirement: "SenderType - 'Tenant' or 'User'"
            // If role is Tenant, SenderType = "Tenant". Else "User".
            
            string senderType = userRole == "Tenant" ? "Tenant" : "User";

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
        public async Task<ActionResult<IEnumerable<MessageResponseDTO>>> GetMessages()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
            string userType = userRole == "Tenant" ? "Tenant" : "User";

            var messages = await _messageService.GetMessagesAsync(userId, userType);
            return Ok(messages);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MessageResponseDTO>> UpdateMessage(int id, [FromBody] UpdateMessageDTO messageDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
            string userType = userRole == "Tenant" ? "Tenant" : "User";

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
    }
}
