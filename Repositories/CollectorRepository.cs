using Api_Loggin.Data;
using Api_Loggin.DTOs;
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

        public async Task<Collector?> AddAsync(RegisterCollectorDto dto)
        {
            var collector = new Collector();
            collector.Url = dto.Url;
            collector.Name = dto.Name;

            await _context.Collectors.AddAsync(collector);
            await _context.SaveChangesAsync();
            return collector;
        }

        public async Task<Collector?> UpdateAsync(UpdateCollectorDto dto)
        {
            var collector = await _context.Collectors.FindAsync(dto.Id);
            if (collector != null)
            {
                collector.Name = dto.Name;
                collector.Url = dto.Url;

                await _context.SaveChangesAsync();
            }
            return collector;
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
