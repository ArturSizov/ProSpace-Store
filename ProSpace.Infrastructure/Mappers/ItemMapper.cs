using ProSpace.Infrastructure.Entites.Supply;
using ProSpace.Domain.Models;

namespace ProSpace.Infrastructure.Mappers
{
    /// <summary>
    /// Item mapper
    /// </summary>
    public static class ItemMapper
    {
        public static ItemEntity ToEntity(this ItemModel model) => new()
        {
            Name = model.Name,
            Code = model.Code,
            Price = model.Price,
            Category = model.Category
        };

        public static ItemModel ToModel(this ItemEntity entity) => new()
        {
            Id = entity.Id,
            Category = entity.Category,
            Code = entity.Code,
            Price = entity.Price,
            Name = entity.Name
        };
    }
}
