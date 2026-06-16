using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using ProSpace.Contracts.Responses;

namespace ProSpace.Api.Infrastructure.Swagger
{
    /// <summary>
    /// Ensures that the default OpenAPI documentation example for BaseResponse shows 'isSuccess: false'.
    /// </summary>
    public class BaseResponseSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// Applies the schema modifications to the generated OpenAPI documentation metadata.
        /// Intercepts the rendering of specific types to inject precise default example models.
        /// </summary>
        /// <param name="schema">The structural OpenAPI schema object being generated for the target component.</param>
        /// <param name="context">The contextual metadata tracking the active C# type reflection properties.</param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(BaseResponse))
            {
                schema.Example = new OpenApiObject
                {
                    ["isSuccess"] = new OpenApiBoolean(false),

                    ["errors"] = new OpenApiArray { new OpenApiString("Error message description.") }
                };
            }
        }

    }
}
