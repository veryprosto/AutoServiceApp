namespace AutoServiceApp.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string Vin { get; set; } = string.Empty;
        public int OwnerId { get; set; }
    }
}