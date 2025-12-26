using YopoBackend.Modules.TermsConditionsCRUD.DTOs;

namespace YopoBackend.Modules.TermsConditionsCRUD.Services
{
    public interface ITermsAndConditionsService
    {
        Task<List<TermsAndConditionResponseDTO>> GetAllAsync();
        Task<TermsAndConditionResponseDTO?> GetByIdAsync(int id);
        Task<TermsAndConditionResponseDTO> CreateAsync(CreateTermsAndConditionDTO dto);
        Task<TermsAndConditionResponseDTO?> UpdateAsync(int id, UpdateTermsAndConditionDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
