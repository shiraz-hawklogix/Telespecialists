using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


using Swashbuckle.Swagger;
using System.Web.Http.Description;

namespace TeleSpecialists.Web.API.Extensions
{

    // Adding oAuth2 in API https://stackoverflow.com/questions/51117655/how-to-use-swagger-in-asp-net-webapi-2-0-with-token-based-authentication
    public class SwaggerAuthTokenOperation : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            swaggerDoc.paths.Add("/token", new PathItem
            {
                post = new Operation
                {
                    tags = new List<string> { "Auth" },
                    consumes = new List<string>
                    {
                        "application/x-www-form-urlencoded"
                    },
                    description = "Generate Authorization token for API requests",
                    summary = "Generate Authorization token for API requests",
                    parameters = new List<Parameter>
                    {
                        new Parameter
                        {
                            type = "string",
                            name = "username",
                            required = true,
                            @in = "formData"
                        },
                        new Parameter
                        {
                            type = "string",
                            name = "password",
                            required = true,
                            @in = "formData"
                        },
                        new Parameter
                        {
                            type = "string",
                            name = "grant_type",
                            required = true,
                            @in = "formData"
                        }
                    },
                    responses = new Dictionary<string, Response>() { { "200", new Response() { description = "OK" } } },
                    

                }
            });
        }
    }
}
