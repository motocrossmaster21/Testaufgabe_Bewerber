using Microsoft.EntityFrameworkCore;
using WeatherMeasurementService.Models;

namespace WeatherMeasurementService.Data
{
    public class WeatherDbContext(DbContextOptions<WeatherDbContext> options) : DbContext(options)
    {

        // DbSets for the entities
        public DbSet<Station> Stations { get; set; }
        public DbSet<MeasurementType> MeasurementTypes { get; set; }
        public DbSet<WeatherData> WeatherData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Station 1:n WeatherData
            modelBuilder.Entity<Station>()
                .HasMany(s => s.Measurements)
                .WithOne(m => m.Station)
                .HasForeignKey(m => m.StationId);

            // MeasurementType 1:n WeatherData
            modelBuilder.Entity<MeasurementType>()
                .HasMany(mt => mt.Measurements)
                .WithOne(m => m.MeasurementType)
                .HasForeignKey(m => m.MeasurementTypeId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
