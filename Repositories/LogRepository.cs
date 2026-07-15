using Api_Loggin.Data;
using Api_Loggin.DTOs;
using Api_Loggin.Models;
using Api_Loggin.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_Loggin.Repositories
{
    public class LogRepository : ILogRepository
    {
        private readonly AppDbContext _context;

        public LogRepository(AppDbContext dbcontext)
        {
            _context = dbcontext;
        }

        public async Task<List<Log>> GetAllAsync()
        {
            return await _context.Logs.Include(l => l.Collector).ToListAsync();
        }

        public async Task<Log?> GetByIdAsync(Guid id)
        {
            return await _context.Logs.Include(l => l.Collector).FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Log?> AddAsync(RegisterLogDto dto)
        {
            var collectorExists = await _context.Collectors.AnyAsync(c => c.Id == dto.CollectorId);
            if (!collectorExists) return null;

            var log = new Log
            {
                CollectorId = dto.CollectorId,
                Path = dto.path
            };

            await _context.Logs.AddAsync(log);
            await _context.SaveChangesAsync();
            return log;
        }

        public async Task<Log?> UpdateAsync(UpdateLogDto dto)
        {
            var log = await _context.Logs.FindAsync(dto.Id);
            if (log != null)
            {
                var collectorExists = await _context.Collectors.AnyAsync(c => c.Id == dto.CollectorId);
                if (!collectorExists) 
                    return null;

                log.Path = dto.Path;
                log.CollectorId = dto.CollectorId;

                await _context.SaveChangesAsync();
            }
            return log;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var log = await _context.Logs.FindAsync(id);
            if (log != null)
            {
                _context.Logs.Remove(log);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<Log?> FetchAsync(FetchLogDto dto)
        {
            return await _context.Logs
            .Where(l => l.CollectorId == dto.CollectorId && l.Id == dto.Id)
            .FirstOrDefaultAsync();
        }
    }
}
