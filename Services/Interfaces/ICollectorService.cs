using Api_Loggin.Models;

namespace Api_Loggin.Services.Interfaces
{
    public interface ICollectorService
    {
        Task<bool> RegisterCollectorAsync(Collector collector);
        Task<List<Collector>> GetAllCollectorsAsync();
        Task<Collector> GetCollectorAsync(Guid id);
        Task<Collector> UpdateCollectorAsync(Collector collector);
        Task<bool> DeleteCollectorAsync(Guid id);
    }
}
