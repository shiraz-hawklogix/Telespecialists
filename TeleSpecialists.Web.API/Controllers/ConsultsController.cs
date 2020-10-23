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
    public class ConsultsController : BaseController
    {
        /// <summary>
        /// Returns the number of cases categorized as a consults per facility according to date range.
        /// </summary>
        /// <param name="start_date">start date to filter records</param>
        /// <param name="end_date">end date to filter records</param>
        /// <returns>List</returns>
        /// <remarks>Returns the number of cases categorized as a consults per facility according to date range.</remarks>
        [HttpGet]
        [Authorize]
        [Route("consults/summary")]
        [SwaggerResponse(HttpStatusCode.OK, "", typeof(Consult))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]
        public IHttpActionResult Summary(DateTime? start_date = null, DateTime? end_date = null)
        {
            try
            {
                if (!start_date.HasValue) return MissingStartDate();
                if (!end_date.HasValue) return MissingEndtDate();

                string query = string.Format("Exec usp_api_consults_summary @StartDate = '{0}', @EndDate = '{1}'", start_date, end_date);

                //var result = _dbContext.SqlToList(query);
                var result = _dbContext.Database.SqlQuery<Consult>(query).ToList();
                return Json(result);
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }

        /// <summary>
        /// Returns the number of cases categorized as a consults by day per facility according to date range.
        /// </summary>
        /// <param name="start_date">start date to filter records</param>
        /// <param name="end_date">end date to filter records</param>
        /// <returns>List</returns>
        /// <remarks>Returns the number of cases categorized as a consults by day per facility according to date range.</remarks>
        [HttpGet]
        [Authorize]
        [Route("consults/by-day")]
        [SwaggerResponse(HttpStatusCode.OK, "", typeof(ConsultByDay))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]
        public IHttpActionResult ByDay(DateTime? start_date = null, DateTime? end_date = null)
        {
            try
            {
                if (!start_date.HasValue) return MissingStartDate();
                if (!end_date.HasValue) return MissingEndtDate();

                string query = string.Format("Exec usp_api_consults_by_day @StartDate = '{0}', @EndDate = '{1}'", start_date, end_date);

                //var result = _dbContext.SqlToList(query);
                var result = _dbContext.Database.SqlQuery<ConsultByDay>(query).ToList();
                return Json(result);
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }
    }
}