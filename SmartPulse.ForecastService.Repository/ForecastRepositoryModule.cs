using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartPulse.ForecastService.Repository.Events;
using SmartPulse.ForecastService.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Repository
{
    public static class ForecastRepositoryModule
    {
        public static IServiceCollection AddForecastRepositoryModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ForecastDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("ForecastDb")));
            services.AddScoped<IForecastRepository, ForecastRepository>();
            services.AddScoped<IPowerPlantRepository, PowerPlantRepository>();
            services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
            return services;
        }
    }
}
