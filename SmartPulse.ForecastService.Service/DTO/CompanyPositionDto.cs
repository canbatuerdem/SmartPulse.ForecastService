using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Service.DTO
{
    public class CompanyPositionDto
    {
        public DateTime HourUtc { get; set; }
        public decimal TotalMWh { get; set; }
    }
}
