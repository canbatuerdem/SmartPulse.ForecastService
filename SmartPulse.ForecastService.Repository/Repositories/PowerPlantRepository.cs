using Microsoft.EntityFrameworkCore;
using SmartPulse.ForecastService.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Repository.Repositories
{
    public class PowerPlantRepository : IPowerPlantRepository
    {
        private readonly ForecastDbContext _context;

        public PowerPlantRepository(ForecastDbContext context)
        {
            _context = context;
        }

        public async Task<PowerPlant?> GetByCodeAsync(string code)
        {
            return await _context.PowerPlants.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code);
        }
    }

}
