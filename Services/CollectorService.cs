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

        public async Task<bool> RegisterCollectorAsync(Collector Collector)
        {
            return await _repo.AddAsync(Collector);
        }
        public async Task<List<Collector>> GetAllCollectorsAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Collector> GetCollectorAsync(Guid id)
        {
            return await _repo.GetByIdAsync(id);
        }
        public async Task<Collector> UpdateCollectorAsync(Collector collector)
        {
            return await _repo.UpdateAsync(collector);
        }
        public async Task<bool> DeleteCollectorAsync(Guid id)
        {
            return await _repo.DeleteAsync(id);
        }
    }
}
