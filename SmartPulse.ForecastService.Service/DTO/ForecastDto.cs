using SmartPulse.ForecastService.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Service.DTO
{
    public class ForecastDto
    {
        public string PlantCode { get; set; } = default!;
        public DateTime ForecastHourUtc { get; set; }
        public decimal QuantityMWh { get; set; }
    }
}
