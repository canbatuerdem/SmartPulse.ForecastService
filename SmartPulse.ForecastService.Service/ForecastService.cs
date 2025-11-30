using Azure.Core;
using Microsoft.Extensions.Logging;
using SmartPulse.ForecastService.Repository.Entities;
using SmartPulse.ForecastService.Repository.Events;
using SmartPulse.ForecastService.Repository.Repositories;
using SmartPulse.ForecastService.Service.DTO;
using SmartPulse.ForecastService.Service.Extensions;
using SmartPulse.ForecastService.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Service
{
    public class ForecastService : IForecastService
    {
        private readonly IForecastRepository _forecastRepository;
        private readonly IPowerPlantRepository _powerPlantRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<ForecastService> _logger;

        public ForecastService(IForecastRepository forecastRepository, IPowerPlantRepository powerPlantRepository, IEventPublisher eventPublisher, ILogger<ForecastService> logger)
        {
            _forecastRepository = forecastRepository;
            _powerPlantRepository = powerPlantRepository;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task UpsertForecastAsync(ForecastDto request)
        {
            DateTime hourUtc = request.ForecastHourUtc.TruncateToHourUtc();
            if (hourUtc < DateTime.UtcNow.TruncateToHourUtc())
            {
                throw new ArgumentException($"ForecastHourUtc cannot be in the past. Value: {hourUtc:O}");
            }

            Guid plantId = await ResolvePlantIdAsync(request.PlantCode);
            Forecast? existingForecast = await _forecastRepository.GetByPlantAndHourAsync(plantId, hourUtc);
            bool positionChanged = false;

            if (existingForecast == null)
            {
                _logger.LogInformation("Creating new forecast. Plant={PlantCode}, HourUtc={HourUtc}, Quantity={Quantity}", request.PlantCode, hourUtc, request.QuantityMWh);
                Forecast forecast = new Forecast()
                {
                    PowerPlantId = plantId,
                    ForecastHourUtc = hourUtc,
                    QuantityMWh = request.QuantityMWh
                };

                await _forecastRepository.AddAsync(forecast);
                positionChanged = true;
            }
            else
            {
                if (existingForecast.QuantityMWh == request.QuantityMWh)
                {
                    _logger.LogInformation("Received identical forecast. No-op. Plant={PlantCode}, HourUtc={HourUtc}, Quantity={Quantity}", request.PlantCode, hourUtc, request.QuantityMWh);
                    return;
                }

                _logger.LogInformation("Updating forecast. Plant={PlantCode}, HourUtc={HourUtc}, Old={OldQuantity}, New={NewQuantity}", request.PlantCode, hourUtc, existingForecast.QuantityMWh, request.QuantityMWh);
                existingForecast.QuantityMWh = request.QuantityMWh;
                await _forecastRepository.UpdateAsync(existingForecast);
                positionChanged = true;
            }

            if (positionChanged)
            {
                await _forecastRepository.SaveChangesAsync();
                _logger.LogInformation("Forecast persisted. Plant={PlantCode}, HourUtc={HourUtc}", request.PlantCode, hourUtc);
                await PublishPositionChangedAsync(hourUtc);
            }
        }

        public async Task<IReadOnlyList<ForecastDto>> GetForecastsAsync(DateTime fromUtc, DateTime toUtc)
        {
            DateTime from = fromUtc.TruncateToHourUtc();
            DateTime to = toUtc.TruncateToHourUtc();

            List<Forecast> list = await _forecastRepository.GetForecastsAsync(from, to);
            var result = list.Select(f => new ForecastDto
            {
                PlantCode = f.PowerPlant.Code,
                ForecastHourUtc = f.ForecastHourUtc,
                QuantityMWh = f.QuantityMWh
            }).ToList();
            return result;
        }

        public async Task<IReadOnlyList<CompanyPositionDto>> GetCompanyPositionAsync(DateTime fromUtc, DateTime toUtc)
        {
            DateTime from = fromUtc.TruncateToHourUtc();
            DateTime to = toUtc.TruncateToHourUtc();

            Dictionary<DateTime, decimal> dict = await _forecastRepository.GetCompanyPositionAsync(from, to);

            return dict
                .Select(x => new CompanyPositionDto
                {
                    HourUtc = x.Key,
                    TotalMWh = x.Value
                })
                .OrderBy(x => x.HourUtc)
                .ToList();
        }

        private async Task PublishPositionChangedAsync(DateTime hourUtc)
        {
            try
            {
                Dictionary<DateTime, decimal> dict = await _forecastRepository.GetCompanyPositionAsync(hourUtc, hourUtc);
                if (!dict.TryGetValue(hourUtc, out var total))
                {
                    total = 0m;
                }    

                _logger.LogInformation("Publishing PositionChanged event. HourUtc={HourUtc}, Total={Total}", hourUtc, total);

                PositionChangedEvent evt = new PositionChangedEvent(hourUtc, total);
                await _eventPublisher.PublishPositionChangedAsync(evt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish PositionChanged event. HourUtc={HourUtc}", hourUtc);
                // Swallow or Rethrow ???
            }
        }

        private async Task<Guid> ResolvePlantIdAsync(string plantCode)
        {
            var plant = await _powerPlantRepository.GetByCodeAsync(plantCode);

            if (plant == null)
            {
                _logger.LogWarning("Unknown PowerPlant code received: {PlantCode}", plantCode);
                throw new ArgumentOutOfRangeException(nameof(plantCode), $"Unknown PowerPlant code: {plantCode}");
            }

            return plant.Id;
        }
    }
}
