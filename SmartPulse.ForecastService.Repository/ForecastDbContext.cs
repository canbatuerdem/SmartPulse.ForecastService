using Microsoft.EntityFrameworkCore;
using SmartPulse.ForecastService.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Repository
{
    public class ForecastDbContext : DbContext
    {
        public ForecastDbContext(DbContextOptions<ForecastDbContext> options) : base(options) 
        {
        }

        public DbSet<Forecast> Forecasts => Set<Forecast>();
        public DbSet<PowerPlant> PowerPlants => Set<PowerPlant>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PowerPlant>().HasIndex(x => x.Code).IsUnique();
            modelBuilder.Entity<PowerPlant>().HasData(
                new PowerPlant
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Code = "TR",
                    Country = "Turkey"
                },
                new PowerPlant
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Code = "BG",
                    Country = "Bulgaria"
                },
                new PowerPlant
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Code = "ES",
                    Country = "Spain"
                }
            );
            modelBuilder.Entity<Forecast>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.PowerPlantId, x.ForecastHourUtc }).IsUnique();
                entity.Property(x => x.QuantityMWh).HasPrecision(18, 4);
                entity.Property<DateTime>("ValidFrom");
                entity.Property<DateTime>("ValidTo");
                entity.ToTable("Forecasts", tb => tb.IsTemporal(t =>
                {
                    t.HasPeriodStart("ValidFrom");
                    t.HasPeriodEnd("ValidTo");
                    t.UseHistoryTable("ForecastsHistory");
                }));
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
