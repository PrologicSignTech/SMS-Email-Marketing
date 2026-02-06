namespace MarketingPlatform.Application.DTOs.Contact
{
    public class ContactDto
    {
        public int Id { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public Dictionary<string, string>? CustomAttributes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Groups { get; set; } = new();
        public List<string> Tags { get; set; } = new();
    }
}
