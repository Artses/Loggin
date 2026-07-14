using Api_Loggin.DTOs;
using Api_Loggin.Models;

namespace Api_Loggin.Services.Interfaces
{
    public interface ICollectorService
    {
        Task<Collector?> RegisterCollectorAsync(RegisterCollectorDto dto);
        Task<List<Collector>> GetAllCollectorsAsync();
        Task<Collector?> GetCollectorAsync(Guid id);
        Task<Collector?> UpdateCollectorAsync(UpdateCollectorDto dto);
        Task<bool> DeleteCollectorAsync(Guid id);
    }
}
