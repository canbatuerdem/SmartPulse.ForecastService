using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Repository.Entities
{
    public class PowerPlant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Code { get; set; }
        public required string Country { get; set; }
    }
}
