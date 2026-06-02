using ProSpace.Infrastructure.Entites.Supply;
using ProSpace.Domain.Models;

namespace ProSpace.Infrastructure.Mappers
{
    /// <summary>
    /// Customer mapper
    /// </summary>
    public static class CustomerMapper
    {
        public static CustomerEntity ToEntity(this CustomerModel model) => new()
        {
            Name = model.Name,
            Code = model.Code,
            Discount = model.Discount,
            Address = model.Address
        };


        public static CustomerModel ToModel(this CustomerEntity entity) => new()
        {
           Id = entity.Id,
           Address = entity.Address,
           Code = entity.Code,
           Discount = entity.Discount,
           Name = entity.Name
        };
    }
}
