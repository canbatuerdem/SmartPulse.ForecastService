using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Repository.Entities
{
    public class Forecast
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PowerPlantId { get; set; }
        public PowerPlant PowerPlant { get; set; }

        public DateTime ForecastHourUtc { get; set; }
        public required decimal QuantityMWh { get; set; }
    }
}
