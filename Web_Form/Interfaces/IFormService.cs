
using Web_Form.Interfaces;
using Web_Form.Models;

namespace Web_Form.Interfaces
{
    public interface IFormService:IBaseService<TblForm>
    {
        Task<IEnumerable<TblForm>> GetFormsByUserAsync(string userId);

        // Retrieve aggregated results for a specific form
      

        // Additional CRUD operations for forms (optional)
        Task AddFormAsync(TblForm form);
        Task UpdateFormAsync(TblForm form);
        Task DeleteFormAsync(int formId);

    }
}
