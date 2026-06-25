using Api_Loggin.Models;
using System.Security.Principal;

namespace Api_Loggin.Repositories.Interfaces
{
    public interface ICollectorRepository
    {
        Task<List<Collector>> GetAllAsync();
        Task<Collector?> GetByIdAsync(Guid id);
        Task<bool> AddAsync(Collector collector);
        Task<Collector> UpdateAsync(Collector collector);
        Task<bool> DeleteAsync(Guid id);
    }
}
