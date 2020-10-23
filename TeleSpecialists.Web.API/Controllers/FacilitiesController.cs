using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TeleSpecialists.Web.API.Extensions;
using TeleSpecialists.Web.API.Models;

namespace TeleSpecialists.Web.API.Controllers
{
    [Authorize]
    public class FacilitiesController : BaseController
    {
        /// <summary>
        /// Returns a list of facilities that provide teleneuro services.
        /// </summary>
        /// <param name="active">active/inactive flag to filter facilities</param>
        /// <returns>List</returns>
        /// <remarks>Returns a list of facilities that provide teleneuro services.</remarks>
        /// <response code="200"></response>
        [HttpGet]
        [Authorize]
        [Route("facilities/teleneuro")]

        //[SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized", typeof(UnauthorizedAccessException))]
        [SwaggerResponse(HttpStatusCode.OK, "OK", typeof(Teleneuro))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]

        public IHttpActionResult Teleneuro(bool active = true)
        {
            try
            {
                string query = string.Format("Exec usp_api_teleneuro_facilities {0}", (active ? 1 : 0));

                //var result = _dbContext.SqlToList(query);
                var result = _dbContext.Database.SqlQuery<Teleneuro>(query).ToList();
                return Json(result);
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }

    }
}