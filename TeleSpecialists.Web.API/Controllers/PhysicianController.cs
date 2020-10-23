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
    public class PhysicianController : BaseController
    {
        /// <summary>
        /// Returns a Physician's schedule according to date range
        /// </summary>
        /// <param name="start_date">Schedule start date to filter records</param>
        /// <param name="end_date">Schedule end date to filter records</param>
        /// <param name="npi">NPI Number of Physician</param>
        /// <returns>List</returns>
        /// <remarks>Returns a Physician's schedule according to date range</remarks>
        /// <response code="200"></response>
        [HttpGet]
        [Authorize]
        [Route("physician/schedule")]

        //[SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized", typeof(string))]
        [SwaggerResponse(HttpStatusCode.OK, "OK", typeof(Schedule))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]

        public IHttpActionResult Schedule(DateTime? start_date = null, DateTime? end_date = null, string npi = "")
        {
            try
            {
                if (!start_date.HasValue) return MissingStartDate();
                if (!end_date.HasValue) return MissingEndtDate();

                string query = string.Format("Exec usp_api_physician_schedule @StartDate = '{0}', @EndDate = '{1}', @NPI = '{2}'", start_date, end_date, npi);

                //var result = _dbContext.SqlToList(query);
                var result = _dbContext.Database.SqlQuery<Schedule>(query).ToList();
                return Json(result);
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }

        /// <summary>
        /// Returns Physician's credentials according to facility
        /// </summary>
        /// <param name="npi">NPI Number of Physician</param>
        /// <param name="source_name">MD Staff Reference Source Name</param>
        /// <param name="facility">TeleCare Facility Name</param>
        /// <param name="onboarded">TeleCare Facility Onboarded Flag</param>
        /// <returns>List</returns>
        /// <remarks>Returns Physician's credentials according to facility</remarks>
        [HttpGet]
        [Authorize]
        [Route("physician/credentials")]

        //[SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized", typeof(string))]
        [SwaggerResponse(HttpStatusCode.OK, "OK", typeof(Credential))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]

        public IHttpActionResult Credentials(string npi = "", string source_name = "", string facility = "", bool onboarded = true)
        {
            try
            {
                int onBoardedFlag = (onboarded ? 1 : 0);
                string query = string.Format("Exec usp_api_physician_credentials @NPI = '{0}', @ReferenceSourceName = '{1}', @FacilityName = '{2}', @Onboarded = {3}", npi, source_name, facility, onBoardedFlag);

                //var result = _dbContext.SqlToList(query);
                var result = _dbContext.Database.SqlQuery<Credential>(query).ToList();
                return Json(result);
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }




        /// <summary>
        /// Returns Physician's licensing by state
        /// </summary>
        /// <param name="npi">NPI Number of Physician</param>
        /// <param name="state">State Physician is Licensed in</param>
        /// <param name="license_type">Physician License Type (e.g. State License, DEA)</param>
        /// <param name="is_valid">Valid/Invalid License</param>
        /// <returns>List</returns>
        /// <remarks>Returns Physician's licensing by state</remarks>
        [HttpGet]
        [Authorize]
        [Route("physician/licensing")]

        //[SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized", typeof(string))]
        [SwaggerResponse(HttpStatusCode.OK, "OK", typeof(Licensing))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]

        public IHttpActionResult Licensing(string npi = "", string state = "", string license_type = "", bool is_valid = true)
        {
            try
            {
                int validFlag = (is_valid ? 1 : 0);
                string query = string.Format("Exec usp_api_physician_license @NPI = '{0}', @State = '{1}', @LicenseType = '{2}', @Valid = {3}", npi, state, license_type, validFlag);

                //var result = _dbContext.SqlToList(query);
                var result = _dbContext.Database.SqlQuery<Licensing>(query).ToList();
                return Json(result);
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }

        /// <summary>
        /// Update credential index of a Physician
        /// </summary>
        /// <param name="npi">NPI Number of Physician</param>
        /// <param name="index">Credential Index Value of Physician</param>
        /// <remarks>Update credential index of a Physician</remarks>

        [Authorize]
        [HttpPut]
        [Route("physician/credential-index")]
        [SwaggerResponse(HttpStatusCode.OK, "OK")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]
        public IHttpActionResult CredentialsIndex(string npi, double index)
        {
            try
            {
                string query = string.Format("Update AspNetUsers set CredentialIndex = {0}, ModifiedDate = GETDATE(), ModifiedByName = 'API User' where NPINumber = '{1}' ", index, npi);

                _dbContext.Database.ExecuteSqlCommand(query);
                //return Ok();
                return Json("Physician Credential Index is Updated.");
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }

        /// <summary>
        /// Authenticate Physicians by username and password
        /// </summary>
        /// <param name="user_name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("physician/Auth")]

        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized", typeof(string))]
        [SwaggerResponse(HttpStatusCode.OK, "OK", typeof(UserModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]

        public IHttpActionResult  Auth(string user_name, string password)
        {
            var user = UserManager.FindAsync(user_name, password).GetAwaiter().GetResult();

            if (user != null)
            {
                var roles = UserManager.GetRolesAsync(user.Id).GetAwaiter().GetResult();
                if (!roles.Select(m=> m.ToLower()).Contains("physician"))
                {
                    return UnAuthorizedRequest("user is not physician");
                }


                string query = string.Format("Exec usp_api_user_by_id @Id = '{0}'", user.Id);

                //var result = _dbContext.SqlToList(query);
                var result = _dbContext.Database.SqlQuery<UserModel>(query).FirstOrDefault();

                if (result != null)
                {
                    return Json(result);
                }
                else
                {
                    return BadRequest("user does not exisits");
                }

               
            }
            else
            {
                return BadRequest("user does not exisits");
            }              
        }


       
        [Authorize]
        [Route("physician/token/save")]

        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized", typeof(string))]
        [SwaggerResponse(HttpStatusCode.OK, "OK", typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]
        [HttpPost]
        public IHttpActionResult SaveToken(string phy_key, string token, string deviceType)
        {
            try { 

                string query = string.Format("Exec [usp_api_user_token] @UserId = '{0}', @Token='{1}', @deviceType='{2}'", phy_key,token, deviceType);

                //var result = _dbContext.SqlToList(query);
                var result = _dbContext.Database.SqlQuery<string>(query).FirstOrDefault();
                 return Json(result);
            }
            catch(Exception ex)
            {
                return ServerError(ex.Message);
            }


        }


    }
}