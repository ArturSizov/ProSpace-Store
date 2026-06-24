using ProSpace.Contracts.DTO.Order;
using ProSpace.Domain.Models;

namespace ProSpace.Application.Mappers
{
    /// <summary>
    /// Order mapper DTO
    /// </summary>
    public static class OrderMapperDto
    {
        public static OrderModel ToModel(this OrderDto dto) => new()
        {
            Id = dto.Id,
            CustomerId = dto.CustomerId,
            OrderDate = dto.OrderDate,
            OrderNumber = dto.OrderNumber,
            ShipmentDate = dto.ShipmentDate,
            Status = dto.Status
        };
            

        public static OrderDto ToDto(this OrderModel model) => new()
        {
            Id = model.Id,
            CustomerId = model.CustomerId,
            OrderDate = model.OrderDate,
            OrderNumber = model.OrderNumber,
            ShipmentDate = model.ShipmentDate,
            Status = model.Status
        };

        public static OrderModel ToDomainEntity(this CreateOrderDto dto) => new OrderModel
        {
           Id = Guid.NewGuid(),
           CustomerId = dto.CustomerId,
           OrderDate = dto.OrderDate,
           OrderNumber= dto.OrderNumber,
           ShipmentDate= dto.ShipmentDate,
           Status = dto.Status
        };

        public static CreateOrderDto ToCreateItemDto(this OrderDto dto) => new CreateOrderDto
        {
            CustomerId = dto.CustomerId,
            OrderDate = dto.OrderDate,
            OrderNumber = dto.OrderNumber,
            ShipmentDate = dto.ShipmentDate,
            Status = dto.Status
        };
    }
}
