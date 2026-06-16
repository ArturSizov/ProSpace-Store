using ProSpace.Domain.Models;

namespace ProSpace.Application.Interfaces.Repositories
{
    public interface IItemsRepository : IBasicCRUD<ItemModel, Guid>
    {
    }
}
