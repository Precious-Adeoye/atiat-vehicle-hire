using AtiatVehicleHire.Models;
using Microsoft.EntityFrameworkCore;

namespace AtiatVehicleHire.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<HireRequest> HireRequests => Set<HireRequest>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
}

public static class SeedData
{
    public static void Seed(AppDbContext db)
    {
        if (db.Vehicles.Any()) return;

        db.Vehicles.AddRange(
            new Vehicle { Name = "Toyota Prado", Type = "SUV", RegistrationNumber = "ATIAT-SUV-01", Status = "Available" },
            new Vehicle { Name = "Toyota Camry", Type = "Sedan", RegistrationNumber = "ATIAT-SED-01", Status = "Available" },
            new Vehicle { Name = "Toyota Hiace", Type = "Bus", RegistrationNumber = "ATIAT-BUS-01", Status = "Available" },
            new Vehicle { Name = "Lexus ES", Type = "Executive Sedan", RegistrationNumber = "ATIAT-EXE-01", Status = "Maintenance" }
        );
        db.SaveChanges();
    }
}
