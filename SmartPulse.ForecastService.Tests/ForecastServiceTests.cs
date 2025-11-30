using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using SmartPulse.ForecastService.Repository.Entities;
using SmartPulse.ForecastService.Repository.Events;
using SmartPulse.ForecastService.Repository.Repositories;
using SmartPulse.ForecastService.Service.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Tests
{
    public class ForecastServiceTests
    {
        private readonly Mock<IForecastRepository> _forecastRepoMock;
        private readonly Mock<IPowerPlantRepository> _plantRepoMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly Mock<ILogger<Service.ForecastService>> _loggerMock;
        private readonly Service.ForecastService _service;

        public ForecastServiceTests()
        {
            _forecastRepoMock = new Mock<IForecastRepository>(MockBehavior.Strict);
            _plantRepoMock = new Mock<IPowerPlantRepository>(MockBehavior.Strict);
            _eventPublisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<Service.ForecastService>>();

            _service = new Service.ForecastService(
                _forecastRepoMock.Object,
                _plantRepoMock.Object,
                _eventPublisherMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task UpsertForecastAsync_NewForecast_InsertsAndPublishesEvent()
        {
            // arrange
            var hour = DateTime.Today.AddDays(1).AddHours(12);

            var request = new ForecastDto
            {
                PlantCode = "TR",
                ForecastHourUtc = hour,
                QuantityMWh = 100m
            };

            // no forecast
            _forecastRepoMock.Setup(r => r.GetByPlantAndHourAsync(It.IsAny<Guid>(), hour)).ReturnsAsync((Forecast?)null);
            _forecastRepoMock.Setup(r => r.AddAsync(It.IsAny<Forecast>())).Returns(Task.CompletedTask);
            _forecastRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _plantRepoMock.Setup(p => p.GetByCodeAsync("TR")).ReturnsAsync(new PowerPlant{ Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Code = "TR", Country = "Turkey" });
            _forecastRepoMock.Setup(r => r.GetCompanyPositionAsync(hour, hour)).ReturnsAsync(new Dictionary<DateTime, decimal> {{ hour, 100m }});
            _eventPublisherMock.Setup(p =>p.PublishPositionChangedAsync(It.Is<PositionChangedEvent>(e => e.HourUtc == hour && e.TotalMWh == 100m))).Returns(Task.CompletedTask);

            // act
            await _service.UpsertForecastAsync(request);

            // assert
            _forecastRepoMock.Verify(r => r.AddAsync(It.Is<Forecast>(f => f.ForecastHourUtc == hour && f.QuantityMWh == 100m)), Times.Once);
            _forecastRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _eventPublisherMock.VerifyAll();
        }

        [Fact]
        public async Task UpsertForecastAsync_SameValue_NoOp_NoEvent()
        {
            // arrange
            var hour = DateTime.Today.AddDays(1).AddHours(12);

            var existing = new Forecast
            {
                PowerPlantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ForecastHourUtc = hour,
                QuantityMWh = 150m
            };

            var request = new ForecastDto
            {
                PlantCode = "TR",
                ForecastHourUtc = hour,
                QuantityMWh = 150m
            };

            _forecastRepoMock.Setup(r => r.GetByPlantAndHourAsync(It.IsAny<Guid>(), hour)).ReturnsAsync(existing);
            _plantRepoMock.Setup(p => p.GetByCodeAsync("TR")).ReturnsAsync(new PowerPlant { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Code = "TR", Country = "Turkey" });

            // act
            await _service.UpsertForecastAsync(request);

            // assert
            _forecastRepoMock.Verify(r => r.AddAsync(It.IsAny<Forecast>()), Times.Never);
            _forecastRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Forecast>()), Times.Never);
            _forecastRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
            _eventPublisherMock.Verify(p => p.PublishPositionChangedAsync(It.IsAny<PositionChangedEvent>()), Times.Never);
        }

        [Fact]
        public async Task UpsertForecastAsync_DifferentValue_UpdatesAndPublishesEvent()
        {
            // arrange
            var hour = DateTime.Today.AddDays(1).AddHours(12);

            var existing = new Forecast
            {
                PowerPlantId = Guid.NewGuid(),
                ForecastHourUtc = hour,
                QuantityMWh = 120m
            };

            var request = new ForecastDto
            {
                PlantCode = "TR",
                ForecastHourUtc = hour,
                QuantityMWh = 140m // diffrent value
            };

            _forecastRepoMock.Setup(r => r.GetByPlantAndHourAsync(It.IsAny<Guid>(), hour)).ReturnsAsync(existing);
            _forecastRepoMock.Setup(r => r.UpdateAsync(existing)).Returns(Task.CompletedTask);
            _forecastRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _forecastRepoMock.Setup(r => r.GetCompanyPositionAsync(hour, hour)).ReturnsAsync(new Dictionary<DateTime, decimal>{{ hour, 140m }});
            _plantRepoMock.Setup(p => p.GetByCodeAsync("TR")).ReturnsAsync(new PowerPlant { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Code = "TR", Country = "Turkey" });
            _eventPublisherMock.Setup(p => p.PublishPositionChangedAsync(It.Is<PositionChangedEvent>(e => e.HourUtc == hour && e.TotalMWh == 140m))).Returns(Task.CompletedTask);

            // act
            await _service.UpsertForecastAsync(request);

            // assert
            Assert.Equal(140m, existing.QuantityMWh);

            _forecastRepoMock.Verify(r => r.UpdateAsync(existing), Times.Once);
            _forecastRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _eventPublisherMock.VerifyAll();
        }

        [Fact]
        public async Task GetCompanyPositionAsync_MapsDictionaryToDtoList()
        {
            var date1 = HourUtc(2025, 11, 28, 10);
            var date2 = HourUtc(2025, 11, 28, 12);

            var dict = new Dictionary<DateTime, decimal> { { date1, 100m }, { date2, 200m }};
            _forecastRepoMock.Setup(r => r.GetCompanyPositionAsync(date1, date2)).ReturnsAsync(dict);
            var result = await _service.GetCompanyPositionAsync(date1, date2);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.HourUtc == date1 && x.TotalMWh == 100m);
            Assert.Contains(result, x => x.HourUtc == date2 && x.TotalMWh == 200m);
        }

        [Fact]
        public async Task UpsertForecastAsync_PastDate_ThrowsException()
        {
            var dto = new ForecastDto
            {
                PlantCode = "TR",
                ForecastHourUtc = DateTime.UtcNow.AddHours(-2),
                QuantityMWh = 100
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpsertForecastAsync(dto));
        }

        private static DateTime HourUtc(int year, int month, int day, int hour) => DateTime.SpecifyKind(new DateTime(year, month, day, hour, 0, 0), DateTimeKind.Utc);
    }
}
