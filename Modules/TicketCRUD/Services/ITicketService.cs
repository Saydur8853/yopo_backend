using YopoBackend.Modules.TicketCRUD.DTOs;

namespace YopoBackend.Modules.TicketCRUD.Services
{
    public interface ITicketService
    {
        Task<(List<TicketResponseDTO> tickets, int totalRecords)> GetTicketsAsync(
            int currentUserId,
            int page = 1,
            int pageSize = 10,
            int? buildingId = null,
            int? unitId = null,
            string? status = null,
            string? concernLevel = null,
            string? searchTerm = null,
            bool includeDeleted = false);

        Task<TicketResponseDTO> CreateTicketAsync(CreateTicketDTO dto, int currentUserId);

        Task<TicketMutationResult> UpdateTicketAsync(int ticketId, UpdateTicketDTO dto, int currentUserId);

        Task<TicketMutationResult> DeleteTicketAsync(int ticketId, int currentUserId);
    }

    public class TicketMutationResult
    {
        public bool NotFound { get; set; }
        public bool Locked { get; set; }
        public string? Message { get; set; }
        public TicketResponseDTO? Ticket { get; set; }
    }
}
