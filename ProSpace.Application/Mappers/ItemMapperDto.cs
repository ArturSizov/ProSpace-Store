using ProSpace.Contracts.DTO;
using ProSpace.Domain.Models;

namespace ProSpace.Application.Mappers
{
    /// <summary>
    /// Item mapper DTO
    /// </summary>
    public static class ItemMapperDto
    {
        public static ItemModel ToModel(this ItemDto dto) => new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Code = dto.Code,
            Category = dto.Category,
            Price = dto.Price
        };


        public static ItemDto ToDto(this ItemModel model) => new()
        {
            Id = model.Id,
            Name = model.Name,
            Code = model.Code,
            Category = model.Category,
            Price = model.Price
        };
    }
}

