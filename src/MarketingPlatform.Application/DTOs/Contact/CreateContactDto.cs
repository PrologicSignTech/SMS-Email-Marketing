namespace MarketingPlatform.Application.DTOs.Contact
{
    public class CreateContactDto
    {
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public Dictionary<string, string>? CustomAttributes { get; set; }
    }
}
