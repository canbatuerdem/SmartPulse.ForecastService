using Microsoft.EntityFrameworkCore;
using SmartPulse.ForecastService.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Repository.Repositories
{
    public class ForecastRepository : IForecastRepository
    {
        private readonly ForecastDbContext _context;

        public ForecastRepository(ForecastDbContext context)
        {
            _context = context;
        }

        public async Task<Forecast?> GetByPlantAndHourAsync(Guid plantId, DateTime hourUtc)
        {
            return await _context.Forecasts
                .FirstOrDefaultAsync(x =>
                    x.PowerPlantId == plantId &&
                    x.ForecastHourUtc == hourUtc);
        }

        public async Task AddAsync(Forecast forecast)
        {
            await _context.Forecasts.AddAsync(forecast);
        }

        public Task UpdateAsync(Forecast forecast)
        {
            _context.Forecasts.Update(forecast);
            return Task.CompletedTask;
        }

        public async Task<List<Forecast>> GetForecastsAsync(DateTime fromUtc, DateTime toUtc)
        {
            return await _context.Forecasts
                .Include(f => f.PowerPlant)
                .Where(x => x.ForecastHourUtc >= fromUtc && x.ForecastHourUtc <= toUtc)
                .ToListAsync();
        }

        public async Task<Dictionary<DateTime, decimal>> GetCompanyPositionAsync(DateTime fromUtc, DateTime toUtc)
        {
            return await _context.Forecasts
                .Where(x => x.ForecastHourUtc >= fromUtc && x.ForecastHourUtc <= toUtc)
                .GroupBy(x => x.ForecastHourUtc)
                .Select(g => new
                {
                    Hour = g.Key,
                    Total = g.Sum(x => x.QuantityMWh)
                })
                .ToDictionaryAsync(x => x.Hour, x => x.Total);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
