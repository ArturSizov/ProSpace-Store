using Microsoft.AspNetCore.Mvc;
using ProSpace.Application.Common.Models;
using ProSpace.Contracts.Contracts.Response;
using ProSpace.Contracts.Responses;

namespace ProSpace.Api.Controllers
{
    /// <summary>
    /// Abstract foundational API controller providing centralized, reusable response processing 
    /// pipelines across all application workflow endpoints.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Automatically maps a generic domain data model response framework packet to its matching HTTP status code.
        /// Primarily handles GET requests that return objects or arrays.
        /// </summary>
        /// <typeparam name="T">The data transfer object payload reference type containing business data.</typeparam>
        /// <param name="response">The internal application service data layer execution envelope instance.</param>
        /// <returns>
        /// An <see cref="IActionResult"/>: <see cref="OkObjectResult"/> (200 OK) containing data,
        /// <see cref="NotFoundObjectResult"/> (404 Not Found), or <see cref="BadRequestObjectResult"/> (400 Bad Request).
        /// </returns>
        protected IActionResult ProcessResponse<T>(BaseResponse<T> response) where T : class
        {
            // Evaluate whether the operational layer pipeline encountered a critical failure
            if (!response.IsSuccess)
            {
                // Scan the text error collection to evaluate if a resource lookup failure occurred
                bool isNotFound = response.Errors is not null && response.Errors.Any(err =>
                    err.Contains("Not found", StringComparison.OrdinalIgnoreCase));

                var errors = response.Errors ?? [];

                // Format the error into the unified non-generic BaseResponse schema matching contract definitions
                if (isNotFound)
                    return NotFound(BaseResponse.Failure(errors));

                return BadRequest(BaseResponse.Failure(errors));
            }

            // Return the populated generic response payload structure inside a standard 200 OK block
            return Ok(response);
        }

        /// <summary>
        /// Automatically maps a specialized identification response framework packet containing only a Guid tracker to its matching HTTP status code.
        /// Primarily handles resource creation and deletion requests.
        /// </summary>
        /// <param name="response">The internal unique identifier transaction container instance.</param>
        /// <returns>
        /// An <see cref="IActionResult"/>: <see cref="OkObjectResult"/> (200 OK) containing the key metadata,
        /// <see cref="NotFoundObjectResult"/> (404 Not Found), or <see cref="BadRequestObjectResult"/> (400 Bad Request).
        /// </returns>
        protected IActionResult ProcessResponse(BaseIdResponse response)
        {
            if (!response.IsSuccess)
            {
                bool isNotFound = response.Errors is not null && response.Errors.Any(err =>
                    err.Contains("Not found", StringComparison.OrdinalIgnoreCase));

                var errors = response.Errors ?? [];

                // Wrap structural array collection entries into an explicit failure response instance tracking model
                if (isNotFound)
                    return NotFound(BaseResponse.Failure(errors));

                return BadRequest(BaseResponse.Failure(errors));
            }

            // Deliver the successful transactional completion model mapping containing only the asset tracker identifier
            return Ok(response);
        }
    }
}
