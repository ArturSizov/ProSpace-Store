namespace ProSpace.Api.Contracts.Response
{
    /// <summary>
    /// Item response
    /// </summary>
    public class ItemResponse
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Category { get; set; }
    }
}
