using Api_Loggin.Data;
using Api_Loggin.Models;
using Api_Loggin.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_Loggin.Repositories
{
    public class CollectorRepository : ICollectorRepository
    {
        private readonly AppDbContext _context;

        public CollectorRepository(AppDbContext dbcontext)
        {
            _context = dbcontext;
        }

        public async Task<List<Collector>> GetAllAsync()
        {
            return await _context.Collectors.ToListAsync();
        }

        public async Task<Collector?> GetByIdAsync(Guid id)
        {
            return await _context.Collectors.FindAsync(id);
        }

        public async Task<bool> AddAsync(Collector collector)
        {
            await _context.Collectors.AddAsync(collector);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Collector> UpdateAsync(Collector collector)
        {
            var existingCollector = await _context.Collectors.FindAsync(collector.Id);
            if (existingCollector != null)
            {
                existingCollector.Name = collector.Name;
                existingCollector.Url = collector.Url;
                existingCollector.Logs = collector.Logs;

                await _context.SaveChangesAsync();

            }
            return existingCollector;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var collector = await _context.Collectors.FindAsync(id);
            if (collector != null)
            {
                _context.Collectors.Remove(collector);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
