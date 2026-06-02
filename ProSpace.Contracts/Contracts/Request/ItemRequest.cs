namespace ProSpace.Api.Contracts.Request
{
    /// <summary>
    /// Item request
    /// </summary>
    public class ItemRequest
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Category { get; set; }
    }
}
