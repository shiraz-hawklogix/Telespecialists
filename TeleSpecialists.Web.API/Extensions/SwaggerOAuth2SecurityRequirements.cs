using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace TeleSpecialists.Web.API.Extensions
{
    public class SwaggerOAuth2SecurityRequirements : IOperationFilter
    {
        /*
        /// <summary>
        /// Defined as per documentation http://wmpratt.com/part-ii-swagger-and-asp-net-web-api-enabling-oauth2/
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="schemaRegistry"></param>
        /// <param name="apiDescription"></param>
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            // Determine if the operation has the Authorize attribute
            var authorizeAttributes = apiDescription
                .ActionDescriptor.GetCustomAttributes<AuthorizeAttribute>();

            if (!authorizeAttributes.Any())
                return;

            // Initialize the operation.security property
            if (operation.security == null)
                operation.security = new List<IDictionary<string, IEnumerable<string>>>();

            // Add the appropriate security definition to the operation
            var oAuthRequirements = new Dictionary<string, IEnumerable<string>>
            {
                { "oauth2", Enumerable.Empty<string>() }
            };

            operation.security.Add(oAuthRequirements);
        }
        */

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            /*
             * As per following article
             * https://stackoverflow.com/questions/51117655/how-to-use-swagger-in-asp-net-webapi-2-0-with-token-based-authentication
            */
            if (operation.parameters != null)
            {
                operation.parameters.Add(new Parameter
                {
                    name = "Authorization",
                    @in = "header",
                    description = "access token",
                    required = true,
                    type = "string"
                });
            }
        }
    }
}