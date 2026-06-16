using ProSpace.Contracts.DTO;
using ProSpace.Domain.Models;

namespace ProSpace.Application.Mappers
{
    /// <summary>
    /// Order item mapper DTO
    /// </summary>
    public static class OrderItemMapperDto
    {
        public static OrderItemModel ToModel(this OrderItemDto dto) => new()
        {
            Id = dto.Id,
            ItemId = dto.ItemId,
            ItemPrice = dto.ItemPrice,
            ItemsCount = dto.ItemsCount,
            OrderId = dto.OrderId
        };


        public static OrderItemDto ToDto(this OrderItemModel model) => new()
        {
            Id = model.Id,
            ItemId = model.ItemId,
            ItemPrice = model.ItemPrice,
            ItemsCount = model.ItemsCount,
            OrderId = model.OrderId
        };

        public static OrderItemModel ToDomainEntity(this CreateOrderItemDto dto) => new OrderItemModel
        {
            Id = Guid.NewGuid(),
            ItemId = dto.ItemId,
            ItemPrice = dto.ItemPrice,
            ItemsCount = dto.ItemsCount,
            OrderId = dto.OrderId
        };

        public static CreateOrderItemDto ToCreateItemDto(this OrderItemDto dto) => new CreateOrderItemDto
        {
            ItemId = dto.ItemId,
            ItemPrice = dto.ItemPrice,
            ItemsCount = dto.ItemsCount,
            OrderId = dto.OrderId
        };
    }
}
