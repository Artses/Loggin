using Api_Loggin.DTOs;
using Api_Loggin.Models;
using System.Security.Principal;

namespace Api_Loggin.Repositories.Interfaces
{
    public interface ICollectorRepository
    {
        Task<List<Collector>> GetAllAsync();
        Task<Collector?> GetByIdAsync(Guid id);
        Task<Collector?> AddAsync(RegisterCollectorDto dto);
        Task<Collector?> UpdateAsync(UpdateCollectorDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
