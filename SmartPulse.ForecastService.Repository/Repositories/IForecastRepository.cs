using SmartPulse.ForecastService.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Repository.Repositories
{
    public interface IForecastRepository
    {
        Task<Forecast?> GetByPlantAndHourAsync(Guid plantId, DateTime hourUtc);
        Task AddAsync(Forecast forecast);
        Task UpdateAsync(Forecast forecast);
        Task<List<Forecast>> GetForecastsAsync(DateTime fromUtc, DateTime toUtc);
        Task<Dictionary<DateTime, decimal>> GetCompanyPositionAsync(DateTime fromUtc, DateTime toUtc);
        Task SaveChangesAsync();
    }
}
