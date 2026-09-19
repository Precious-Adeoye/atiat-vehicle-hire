using AtiatVehicleHire.Data;
using AtiatVehicleHire.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=atiat.db"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins = builder.Configuration["FrontendUrl"]
            ?? "https://atiat-vehicle-hire.vercel.app";

        policy
            .WithOrigins(
                origins.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries
                )
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
var app = builder.Build();

app.UseCors("Frontend");
app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Seed(db);
}

app.MapGet("/", () => Results.Ok(new
{
    application = "ATIAT Vehicle Hire API",
    status = "running",
    version = "1.0"
}));

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", time = DateTime.UtcNow }));

app.MapGet("/api/vehicles", async (AppDbContext db) =>
    Results.Ok(await db.Vehicles.AsNoTracking().OrderBy(v => v.Name).ToListAsync()));

app.MapGet("/api/requests", async (HttpRequest request, AppDbContext db) =>
{
    if (!IsAdmin(request, builder.Configuration)) return Results.Unauthorized();

    var requests = await db.HireRequests.AsNoTracking()
        .Include(x => x.Vehicle)
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync();

    return Results.Ok(requests);
});

app.MapGet("/api/dashboard/summary", async (HttpRequest request, AppDbContext db) =>
{
    if (!IsAdmin(request, builder.Configuration)) return Results.Unauthorized();

    var requests = await db.HireRequests.AsNoTracking().ToListAsync();
    return Results.Ok(new
    {
        total = requests.Count,
        pending = requests.Count(x => x.Status == "Pending"),
        reviewed = requests.Count(x => x.Status == "Reviewed"),
        quoted = requests.Count(x => x.Status == "Quoted"),
        confirmed = requests.Count(x => x.Status == "Confirmed"),
        completed = requests.Count(x => x.Status == "Completed"),
        cancelled = requests.Count(x => x.Status == "Cancelled")
    });
});

app.MapPost("/api/requests", async (HireRequest input, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(input.FullName) ||
        string.IsNullOrWhiteSpace(input.PhoneNumber) ||
        string.IsNullOrWhiteSpace(input.PickupLocation) ||
        string.IsNullOrWhiteSpace(input.Destination) ||
        input.HireDate == default ||
        string.IsNullOrWhiteSpace(input.PickupTime))
    {
        return Results.BadRequest(new { message = "Please complete all required fields." });
    }

    if (input.HireDate.Date < DateTime.UtcNow.Date)
        return Results.BadRequest(new { message = "Hire date cannot be in the past." });

    var reference = await GenerateReference(db);

    var request = new HireRequest
    {
        ReferenceNumber = reference,
        FullName = input.FullName.Trim(),
        PhoneNumber = input.PhoneNumber.Trim(),
        Email = input.Email?.Trim(),
        PickupLocation = input.PickupLocation.Trim(),
        Destination = input.Destination.Trim(),
        HireDate = input.HireDate.Date,
        PickupTime = input.PickupTime.Trim(),
        Duration = input.Duration?.Trim(),
        Passengers = input.Passengers,
        VehiclePreference = input.VehiclePreference?.Trim(),
        DriverRequired = input.DriverRequired,
        Purpose = input.Purpose?.Trim(),
        AdditionalInformation = input.AdditionalInformation?.Trim(),
        Status = "Pending",
        CreatedAt = DateTime.UtcNow
    };

    db.HireRequests.Add(request);
    await db.SaveChangesAsync();

    return Results.Created($"/api/requests/{request.Id}", new
    {
        message = "Your vehicle-hire request has been received.",
        referenceNumber = request.ReferenceNumber
    });
});

app.MapPatch("/api/requests/{id:int}/status", async (int id, StatusUpdate update, HttpRequest request, AppDbContext db) =>
{
    if (!IsAdmin(request, builder.Configuration)) return Results.Unauthorized();

    var allowed = new[] { "Pending", "Reviewed", "Quoted", "Confirmed", "Completed", "Cancelled" };
    if (!allowed.Contains(update.Status))
        return Results.BadRequest(new { message = "Invalid status." });

    var item = await db.HireRequests.FindAsync(id);
    if (item is null) return Results.NotFound(new { message = "Request not found." });

    item.Status = update.Status;
    item.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();

    return Results.Ok(item);
});

app.MapPatch("/api/vehicles/{id:int}", async (int id, VehicleUpdate update, HttpRequest request, AppDbContext db) =>
{
    if (!IsAdmin(request, builder.Configuration)) return Results.Unauthorized();

    var vehicle = await db.Vehicles.FindAsync(id);
    if (vehicle is null) return Results.NotFound(new { message = "Vehicle not found." });

    if (!string.IsNullOrWhiteSpace(update.Status))
    {
        var allowed = new[] { "Available", "On Hire", "Maintenance" };
        if (!allowed.Contains(update.Status)) return Results.BadRequest(new { message = "Invalid vehicle status." });
        vehicle.Status = update.Status;
    }

    if (!string.IsNullOrWhiteSpace(update.Name)) vehicle.Name = update.Name.Trim();
    if (!string.IsNullOrWhiteSpace(update.Type)) vehicle.Type = update.Type.Trim();
    if (!string.IsNullOrWhiteSpace(update.RegistrationNumber)) vehicle.RegistrationNumber = update.RegistrationNumber.Trim();

    await db.SaveChangesAsync();
    return Results.Ok(vehicle);
});

app.Run();

static bool IsAdmin(HttpRequest request, IConfiguration configuration)
{
    var configuredKey = configuration["AdminKey"] ?? "ATIAT-DEMO-2026";
    return request.Headers.TryGetValue("X-Admin-Key", out var supplied) && supplied == configuredKey;
}

static async Task<string> GenerateReference(AppDbContext db)
{
    var year = DateTime.UtcNow.Year;
    var count = await db.HireRequests.CountAsync() + 1;
    return $"ATIAT-HR-{year}-{count:D4}";
}

record StatusUpdate(string Status);
record VehicleUpdate(string? Name, string? Type, string? RegistrationNumber, string? Status);
