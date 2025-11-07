namespace AgroTech.Application.DTOs
{
    public class FarmDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    public class CropDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime PlantingDate { get; set; }
        public DateTime? HarvestDate { get; set; }
        public Guid FarmId { get; set; }
    }

    public class UserDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Producer";
    }
}