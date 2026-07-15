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
            _collectorRepo = collectorRepo;
            _repo = repo;
        }

        public async Task<string> FetchLogAsync(FetchLogDto dto)
        {
            var log = await _repo.FetchAsync(dto);
            if (log == null)
            {
                return "Log not found or invalid collector association.";
            }

            var collector = await _collectorRepo.GetByIdAsync(dto.CollectorId);
            if (collector == null)
            {
                return "Associated collector not found.";
            }

            try
            {
                var req = new RequestLogDto(log.Path); //Add line num

                var fullUrl = $"http://{collector.Url}/api/v1/logs";

                var res = await _httpClient.PostAsJsonAsync(
                fullUrl,
                req);

                return await res.Content.ReadAsStringAsync();
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
