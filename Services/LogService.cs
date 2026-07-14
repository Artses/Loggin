using Api_Loggin.DTOs;
using Api_Loggin.Models;
using Api_Loggin.Repositories.Interfaces;
using Api_Loggin.Services.Interfaces;

namespace Api_Loggin.Services
{
    public class LogService : ILogService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogRepository _repo;
        private readonly ICollectorRepository _collectorRepo;

        public LogService(HttpClient httpClient, ILogRepository repo, ICollectorRepository collectorRepo)
        {
            _httpClient = httpClient;
            _repo = repo;
            _collectorRepo = collectorRepo;
        }

        public async Task<string> FetchLogAsync(GetLogDto dto)
        {
            var log = await _repo.GetByIdAsync(dto.LogId);
            if (log == null || log.CollectorId != dto.CollectorId)
            {
                return "Log not found or invalid collector association.";
            }

            var collector = log.Collector ?? await _collectorRepo.GetByIdAsync(dto.CollectorId);
            if (collector == null)
            {
                return "Associated collector not found.";
            }

            try
            {
                var baseUrl = collector.Url.TrimEnd('/');
                var path = log.Path.StartsWith('/') ? log.Path : "/" + log.Path;
                var fullUrl = $"{baseUrl}{path}";

                return await _httpClient.GetStringAsync(fullUrl);
            }
            catch (Exception ex)
            {
                return $"Error fetching log: {ex.Message}";
            }
        }

        public async Task<Log?> RegisterLogAsync(RegisterLogDto dto)
        {
            return await _repo.AddAsync(dto);
        }

        public async Task<List<Log>> GetAllLogsAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Log?> GetLogAsync(Guid id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<Log?> UpdateLogAsync(UpdateLogDto dto)
        {
            return await _repo.UpdateAsync(dto);
        }

        public async Task<bool> DeleteLogAsync(Guid id)
        {
            return await _repo.DeleteAsync(id);
        }
    }
}
