using ProSpace.Contracts.DTO.Customer;
using ProSpace.Domain.Models;

namespace ProSpace.Application.Mappers
{

    /// <summary>
    /// Customer mapper DTO
    /// </summary>
    public static class CustomerMapperDto
    {
        public static CustomerModel ToModel(this CustomerDto dto) => new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Address = dto.Address,
            Code = dto.Code,
            Discount = dto.Discount
        };


        public static CustomerDto ToDto(this CustomerModel model) => new()
        {
            Id = model.Id,
            Name = model.Name,
            Address = model.Address,
            Code = model.Code,
            Discount = model.Discount
        };
    }
}
