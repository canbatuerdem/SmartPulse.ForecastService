using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Service.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime TruncateToHourUtc(this DateTime value)
        {
            var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            return new DateTime(
                utc.Year,
                utc.Month,
                utc.Day,
                utc.Hour,
                0,
                0,
                DateTimeKind.Utc);
        }
    }

}
