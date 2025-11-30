using SmartPulse.ForecastService.Repository.Entities;
using SmartPulse.ForecastService.Service.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Service.Interfaces
{
    public interface IForecastService
    {
        Task UpsertForecastAsync(ForecastDto request);
        Task<IReadOnlyList<ForecastDto>> GetForecastsAsync(DateTime fromUtc, DateTime toUtc);
        Task<IReadOnlyList<CompanyPositionDto>> GetCompanyPositionAsync(DateTime fromUtc, DateTime toUtc);
    }
}
