using ProSpace.Infrastructure.Entites.Users;

namespace ProSpace.Infrastructure.Entites.Supply
{
    /// <summary>
    /// Customer
    /// </summary>
    public class CustomerEntity
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
        public string? Address { get; set; }
        public decimal? Discount { get; set; }
        public ICollection<OrderEntity> Orders { get; set; } = [];
        public AppUser AppUser { get; set; } = null!;
    }

}
