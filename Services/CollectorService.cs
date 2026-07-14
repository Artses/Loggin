using Api_Loggin.DTOs;
using Api_Loggin.Models;
using Api_Loggin.Repositories.Interfaces;
using Api_Loggin.Services.Interfaces;

namespace Api_Loggin.Services
{
    public class CollectorService : ICollectorService
    {
        private readonly ICollectorRepository _repo;

        public CollectorService(ICollectorRepository repo)
        {
            _repo = repo;
        }

        public async Task<Collector?> RegisterCollectorAsync(RegisterCollectorDto dto)
        {
            return await _repo.AddAsync(dto);
        }
        public async Task<List<Collector>> GetAllCollectorsAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Collector?> GetCollectorAsync(Guid id)
        {
            return await _repo.GetByIdAsync(id);
        }
        public async Task<Collector?> UpdateCollectorAsync(UpdateCollectorDto dto)
        {
            return await _repo.UpdateAsync(dto);
        }
        public async Task<bool> DeleteCollectorAsync(Guid id)
        {
            return await _repo.DeleteAsync(id);
        }
    }
}
