using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Repository.Events
{
    public interface IEventPublisher
    {
        Task PublishPositionChangedAsync(PositionChangedEvent evt);
    }
}
