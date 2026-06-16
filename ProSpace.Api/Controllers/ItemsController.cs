using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProSpace.Api.Contracts.Request;
using ProSpace.Application.Interfaces.Services;
using ProSpace.Contracts.DTO;
using ProSpace.Contracts.Responses;

namespace ProSpace.Api.Controllers
{
    /// <summary>
    /// Provides HTTP endpoints for managing the product catalog, including creating, 
    /// retrieving, updating, and deleting inventory items.
    /// </summary>
    public class ItemsController : BaseApiController
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<ItemsController> _logger;

        /// <summary>
        /// Items service
        /// </summary>
        private readonly IItemsService _service;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        public ItemsController(ILogger<ItemsController> logger, IItemsService service)
        {
            _logger = logger;
            _service = service;
        }

        /// <summary>
        /// Creates and registers a new catalog item in the system.
        /// </summary>
        /// <param name="request">The item data transfer object containing product specifications, stock metrics, or prices.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper containing the operational status and generated identifier data.</returns>
        /// <response code="200">The catalog item was validated and registered successfully.</response>
        /// <response code="400">The input fields failed structural layout rules or validation checks.</response>
        /// <response code="401">The request lacks valid authentication credentials.</response>
        [HttpPost]
        [Authorize(Roles = "manager,Manager")]
        [ProducesResponseType(typeof(BaseIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] ItemRequest request, CancellationToken ct)
        {
            var targetDto = new ItemDto
            {
                Id = Guid.NewGuid(),
                Category = request.Category,
                Code = request.Code,
                Name = request.Name,
                Price = request.Price
            };

            return ProcessResponse(await _service.CreateAsync(targetDto, ct));
        }

        /// <summary>
        /// Retrieves the configuration details of a specific catalog item using its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the target catalog item.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper containing the item metrics payload inside the Data field.</returns>
        /// <response code="200">The catalog item record was successfully located and fetched.</response>
        /// <response code="404">No item record matches the provided unique identifier.</response>
        /// /// <response code="401">The request lacks valid authentication credentials.</response>
        [HttpGet("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(BaseResponse<ItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => ProcessResponse(await _service.ReadAsync(id, ct));

        /// <summary>
        /// Retrieves a complete inventory list of all catalog items configured in the system.
        /// </summary>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper containing the array of catalog items inside the Data field.</returns>
        /// <response code="200">The item list was built successfully (can return an empty array if no catalog objects exist).</response>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<ItemDto[]>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => ProcessResponse(await _service.ReadAllAsync(ct));

        /// <summary>
        /// Updates the key parameters (such as pricing or naming structures) of an existing catalog item.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the item to update. Must match the ID inside the payload framework.</param>
        /// <param name="request">The item data transfer object holding the revised entity parameters.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper delivering the freshly processed item details.</returns>
        /// <response code="200">The target item fields were located, mapped, and updated securely.</response>
        /// <response code="400">The boundary path ID does not align with the object inner ID, or data rules failed.</response>
        /// <response code="401">The request lacks valid authentication credentials.</response>
        /// <response code="404">The catalog item matching the requested identifier cannot be found.</response>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "manager,Manager")]
        [ProducesResponseType(typeof(BaseIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] ItemRequest request, CancellationToken ct)
        {
            var targetItem = new ItemDto
            {
                Id = id,
                Code = request.Code,
                Category = request.Category,
                Name = request.Name,
                Price = request.Price
            };

            return ProcessResponse(await _service.UpdateAsync(targetItem, ct));
        }

        /// <summary>
        /// Permanently deletes an item profile from the product catalog database.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the item record to delete.</param>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>A unified response wrapper confirming execution steps via the Id field.</returns>
        /// <response code="200">The product record was purged completely from the data storage layers.</response>
        /// <response code="401">The request lacks valid authentication credentials.</response>
        /// <response code="404">No item record matches the provided identifier to perform deletion logic.</response>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "manager,Manager")]
        [ProducesResponseType(typeof(BaseIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
            => ProcessResponse(await _service.DeleteAsync(id, ct));
    }
}
