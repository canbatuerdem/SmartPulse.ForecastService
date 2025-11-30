using SmartPulse.ForecastService.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Repository.Repositories
{
    public interface IPowerPlantRepository
    {
        Task<PowerPlant?> GetByCodeAsync(string code);
    }
}
