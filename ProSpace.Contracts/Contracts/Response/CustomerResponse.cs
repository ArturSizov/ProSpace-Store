namespace ProSpace.Api.Contracts.Response
{
    /// <summary>
    /// Customer response
    /// </summary>
    public class CustomerResponse
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Address { get; set; }
        public decimal? Discount { get; set; }
    }
}
