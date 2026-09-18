namespace AtiatVehicleHire.Models;

public class HireRequest
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public string PickupTime { get; set; } = string.Empty;
    public string? Duration { get; set; }
    public int Passengers { get; set; }
    public string? VehiclePreference { get; set; }
    public bool DriverRequired { get; set; }
    public string? Purpose { get; set; }
    public string? AdditionalInformation { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
}
