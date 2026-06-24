using ProSpace.Contracts.DTO;
using ProSpace.Contracts.Responses;

namespace ProSpace.Application.Interfaces.Services
{
    /// <summary>
    /// Defines business logic operations for managing catalog items (products) using a unified generic response framework.
    /// </summary>
    public interface IItemsService
    {
        /// <summary>
        /// Creates a new catalog item in the system.
        /// </summary>
        /// <param name="item">The data transfer object containing the new item details.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseIdResponse"/> containing the operation status and the generated ID of the new item.</returns>
        Task<BaseIdResponse> CreateAsync(ItemDto item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a specific catalog item by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the item.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{ItemDto}"/> with the item details inside the Data field, or a failure message if not found.</returns>
        Task<BaseResponse<ItemDto>> ReadAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of all catalog items available in the system.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{ItemDto[]}"/> containing the collection of all items within the Items field.</returns>
        /// <remarks>
        /// WARNING: As the product catalog grows, this method should be replaced 
        /// with a paginated approach to prevent high memory consumption and slow database response times.
        /// </remarks>
        Task<BaseResponse<IEnumerable<ItemDto>>> ReadAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the details of an existing catalog item.
        /// </summary>
        /// <param name="item">The data transfer object containing updated details (must include a valid Id).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="BaseResponse{ItemDto}"/> indicating the update operation status and containing the updated item data payload.</returns>
        Task<BaseResponse<ItemDto>> UpdateAsync(ItemDto item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Completely deletes a catalog item from the database.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the item to be deleted.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref=BaseIdResponse"/> indicating whether the deletion step was fully completed via the Id field.</returns>
        Task<BaseIdResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    }
}
