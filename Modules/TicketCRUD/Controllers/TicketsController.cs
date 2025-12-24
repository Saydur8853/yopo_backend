using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.Attributes;
using YopoBackend.Constants;
using YopoBackend.DTOs;
using YopoBackend.Modules.TicketCRUD.DTOs;
using YopoBackend.Modules.TicketCRUD.Services;

namespace YopoBackend.Modules.TicketCRUD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("14-Tickets")]
    [Authorize]
    [RequireModule(ModuleConstants.TICKET_MODULE_ID)]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<TicketResponseDTO>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<PaginatedResponse<TicketResponseDTO>>> GetTickets(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? buildingId = null,
            [FromQuery] int? unitId = null,
            [FromQuery] string? status = null,
            [FromQuery] string? concernLevel = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool includeDeleted = false)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized(new { message = "User authentication required." });
            }

            var (tickets, totalRecords) = await _ticketService.GetTicketsAsync(
                currentUserId.Value,
                page,
                pageSize,
                buildingId,
                unitId,
                status,
                concernLevel,
                searchTerm,
                includeDeleted);

            var response = new PaginatedResponse<TicketResponseDTO>(tickets, totalRecords, page, pageSize);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TicketResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<TicketResponseDTO>> CreateTicket([FromBody] CreateTicketDTO dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized(new { message = "User authentication required." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _ticketService.CreateTicketAsync(dto, currentUserId.Value);
                return CreatedAtAction(nameof(GetTickets), new { ticketId = created.TicketId }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(TicketResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<TicketResponseDTO>> UpdateTicket(int id, [FromBody] UpdateTicketDTO dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized(new { message = "User authentication required." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _ticketService.UpdateTicketAsync(id, dto, currentUserId.Value);
                if (result.NotFound)
                {
                    return NotFound(new { message = $"Ticket with ID {id} not found or not accessible." });
                }
                if (result.Locked)
                {
                    return Conflict(new { message = result.Message ?? "Ticket is locked." });
                }

                return Ok(result.Ticket);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<ActionResult> DeleteTicket(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized(new { message = "User authentication required." });
            }

            var result = await _ticketService.DeleteTicketAsync(id, currentUserId.Value);
            if (result.NotFound)
            {
                return NotFound(new { message = $"Ticket with ID {id} not found or not accessible." });
            }
            if (result.Locked)
            {
                return Conflict(new { message = result.Message ?? "Ticket is locked." });
            }

            return Ok(new { message = $"Ticket with ID {id} has been deleted." });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
