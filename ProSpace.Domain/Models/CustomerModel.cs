namespace ProSpace.Domain.Models
{
    /// <summary>
    /// Customer model
    /// </summary>
    public class CustomerModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Address { get; set; }
        public decimal? Discount { get; set; }
        public Guid AppUserId { get; set; }
    }
}
