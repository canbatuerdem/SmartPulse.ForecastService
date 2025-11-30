using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartPulse.ForecastService.Repository;
using SmartPulse.ForecastService.Repository.Events;
using SmartPulse.ForecastService.Repository.Repositories;
using SmartPulse.ForecastService.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Service
{
    public static class ForecastServiceModule
    {
        public static IServiceCollection AddForecastServiceModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IForecastService, ForecastService>();
            services.AddForecastRepositoryModule(configuration);
            return services;
        }
    }
}
