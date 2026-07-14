using Api_Loggin.DTOs;
using Api_Loggin.Models;

namespace Api_Loggin.Repositories.Interfaces
{
    public interface ILogRepository
    {
        Task<List<Log>> GetAllAsync();
        Task<Log?> GetByIdAsync(Guid id);
        Task<Log?> AddAsync(RegisterLogDto dto);
        Task<Log?> UpdateAsync(UpdateLogDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
