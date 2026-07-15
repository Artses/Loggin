using Api_Loggin.DTOs;
using Api_Loggin.Models;

namespace Api_Loggin.Services.Interfaces
{
    public interface ILogService
    {
        Task<List<LogsDto>> FetchLogAsync(FetchLogDto dto);
        Task<Log?> RegisterLogAsync(RegisterLogDto dto);
        Task<List<Log>> GetAllLogsAsync();
        Task<Log?> GetLogAsync(Guid id);
        Task<Log?> UpdateLogAsync(UpdateLogDto dto);
        Task<bool> DeleteLogAsync(Guid id);
    }
}
