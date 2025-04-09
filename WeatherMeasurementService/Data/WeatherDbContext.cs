using Microsoft.EntityFrameworkCore;
using WeatherMeasurementService.Models.Daos;

namespace WeatherMeasurementService.Data
{
    public class WeatherDbContext(DbContextOptions<WeatherDbContext> options) : DbContext(options)
    {

        // DbSets for the entities
        public DbSet<StationDao> Stations { get; set; }
        public DbSet<MeasurementTypeDao> MeasurementTypes { get; set; }
        public DbSet<WeatherDataDao> WeatherData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeasurementTypeDao>()
                .HasKey(m => m.MeasurementTypeId);

            modelBuilder.Entity<StationDao>()
                .HasKey(s => s.StationId);

            modelBuilder.Entity<WeatherDataDao>()
                .HasKey(w => w.WeatherDataId);

            // StationDao 1:n WeatherData
            modelBuilder.Entity<StationDao>()
                .HasMany(s => s.Measurements)
                .WithOne(m => m.Station)
                .HasForeignKey(m => m.StationId);

            // MeasurementTypeDao 1:n WeatherData
            modelBuilder.Entity<MeasurementTypeDao>()
                .HasMany(mt => mt.Measurements)
                .WithOne(m => m.MeasurementType)
                .HasForeignKey(m => m.MeasurementTypeId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
