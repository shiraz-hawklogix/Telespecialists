using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using TeleSpecialists.Web.API.Extensions;
using TeleSpecialists.Web.API.Models;
using TeleSpecialists.Web.API.ViewModels;
using TeleSpecialists.BLL.ViewModels;


namespace TeleSpecialists.Web.API.Controllers
{
    [Authorize]
    public class CasesController : BaseController
    {
        /// <summary>
        /// Returns the number of cases categorized as a stroke alert per facility according to date range.
        /// </summary>
        /// <param name="start_date">start date to filter records</param>
        /// <param name="end_date">end date to filter records</param>
        /// <returns>List</returns>
        /// <remarks>Returns the number of cases categorized as a stroke alert per facility according to date range.</remarks>
        [HttpGet]
        [Authorize]
        [Route("cases/stroke-alert/summary")]
        [SwaggerResponse(HttpStatusCode.OK, "OK", typeof(Cases))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]
        public IHttpActionResult Summary(DateTime? start_date = null, DateTime? end_date = null)
        {
            try
            {
                if (!start_date.HasValue) return MissingStartDate();
                if (!end_date.HasValue) return MissingEndtDate();

                string query = string.Format("Exec usp_api_stroke_alert_summary @StartDate = '{0}', @EndDate = '{1}'", start_date, end_date);
                var result = _dbContext.Database.SqlQuery<Cases>(query).ToList();

                //var result = _dbContext.SqlToList(query);
                return Json(result);
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }

        /// <summary>
        /// Returns the number of cases categorized as a stroke alert by day per facility according to date range.
        /// </summary>
        /// <param name="start_date">start date to filter records</param>
        /// <param name="end_date">end date to filter records</param>
        /// <returns>List</returns>
        /// <remarks>Returns the number of cases categorized as a stroke alert by day per facility according to date range.</remarks>
        [HttpGet]
        [Authorize]
        [Route("cases/stroke-alert/by-day")]
        [SwaggerResponse(HttpStatusCode.OK, "", typeof(CasesByDay))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]
        public IHttpActionResult ByDay(DateTime? start_date = null, DateTime? end_date = null)
        {
            try
            {
                if (!start_date.HasValue) return MissingStartDate();
                if (!end_date.HasValue) return MissingEndtDate();

                string query = string.Format("Exec usp_api_stroke_alert_by_day @StartDate = '{0}', @EndDate = '{1}'", start_date, end_date);

                return Json(_dbContext.Database.SqlQuery<CasesByDay>(query).ToList());

                //var result = _dbContext.SqlToList(query);
                //return Json(result);
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }


        /// <summary>
        /// Returns the number of cases categorized as a stroke alert per facility according to date.
        /// </summary>
        /// <param name="case_date">case date to filter records</param>
        /// <returns>List</returns>
        /// <remarks>Returns the number of cases categorized as a stroke alert per facility according to date.</remarks>
        [HttpGet]
        [Authorize]
        [Route("cases/stroke-alert/by-day/detail")]
        [SwaggerResponse(HttpStatusCode.OK, "", typeof(CaseDetail))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]
        public IHttpActionResult ByDayDetail(DateTime? case_date)
        {
            try
            {
                string query = string.Format("Exec usp_api_stroke_alert_by_day_detail @CaseDate = '{0}'", case_date);

                return Json(_dbContext.Database.SqlQuery<CaseDetail>(query).ToList());
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }


        /// <summary>
        /// Return cases list by phyician. All the parmeters are optional
        /// </summary>
        /// <param name="phyId"></param>
        /// <param name="caseType"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="pageSize"></param>
        /// <param name="skip"></param>
        /// <returns></returns>

        [HttpGet]
        [Authorize]
        [Route("cases/list/by-physicain")]
        [SwaggerResponse(HttpStatusCode.OK, "", typeof(CaseByPhysicianResult))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]
        public IHttpActionResult ByPhysician(string  caseType = "", string phyId = "",  DateTime? startDate = null, DateTime? endDate = null, int pageSize = 50, int skip = 0)
        {
            try
            {
                var queryBuilder = new StringBuilder();
                queryBuilder.Append($"@Take={pageSize},@Skip={skip}");
                if (!string.IsNullOrEmpty(phyId))
                    queryBuilder.Append(string.Format(",@Physician='{0}'", phyId));

                if (caseType != null)
                    queryBuilder.Append(string.Format(",@CaseType='{0}'", caseType));

                if (startDate.HasValue)
                    queryBuilder.Append(string.Format(",@StartDate='{0}'", startDate.Value));

                if (endDate.HasValue)
                    queryBuilder.Append(string.Format(",@EndDate='{0}'", endDate.Value));

                var list = _dbContext.Database.SqlQuery<CaseDbModel>("Exec usp_api_cases_by_phy " + queryBuilder.ToString()).ToList();
                var caseByType = list.Select(m => new CaseByPhysician
                {
                    Id = m.cas_key,
                    CaseNumber = m.cas_case_number,
                    CaseStatus = new KeyValType { Id = m.cas_cst_key.ToString(), Name = m.cst_name },
                    CaseType = new KeyValType { Id = m.cas_ctp_key.ToString(), Name = m.ctp_name },
                    Facility = new KeyValType { Id = m.cas_fac_key.ToString(), Name = m.fac_name },
                    Physician = new KeyValType { Id = m.cas_phy_key, Name = m.phy_name },
                    CreatedDate = m.cas_created_date

                }).ToList();
                return Json(new CaseByPhysicianResult { totalRecords = (list.Count() > 0 ? list.First().totalRecords : 0), cases = caseByType });
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("case/detail/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, "", typeof(CaseCustomDetail))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]
        public IHttpActionResult Detail(int Id)
        {
            try
            {
                var queryBuilder = new StringBuilder();
                if (Id != 0)
                    queryBuilder.Append(string.Format("@id='{0}'", Id));
                var result = _dbContext.Database.SqlQuery<CaseCustomDetail>("Exec usp_api_get_case_with_id " + queryBuilder.ToString()).FirstOrDefault();
                //string query = string.Format("Exec  [dbo].[usp_api_cases_by_phy] {0}", Id);
                //var result = new CaseCustomDetail();//_dbContext.Database.SqlQuery<CaseCustomDetail>(query).FirstOrDefault();
                //if(result != null)
                //{
                //    if(result.cas_identification_type == null)
                //    {
                //        result.cas_identification_type = "";
                //    }
                //}
                return Json(result);
                
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }


        [Authorize]
        [HttpPut]
        [Route("case/accept")]
        [SwaggerResponse(HttpStatusCode.OK, "", typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]
        public IHttpActionResult Accept(int Id, string phyId)
        {
            try
            {
                string query = string.Format("Exec  [dbo].[usp_api_case_accept] {0}, '{1}'", Id, phyId);
                var result = _dbContext.Database.SqlQuery<string>(query).FirstOrDefault();

                if (result.ToLower() == "success")
                    _dbContext.Database.ExecuteSqlCommand(string.Format("Exec usp_case_timestamp_calc_update @cas_key='{0}'" , Id));

                return Json(result);
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }


        [Authorize]
        [HttpPut]
        [Route("case/reject")]
        [SwaggerResponse(HttpStatusCode.OK, "", typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]
        public IHttpActionResult Reject(int Id, string phyId)
        {
            try
            {
                string query = string.Format("Exec  [dbo].[usp_api_case_reject] {0}, '{1}'", Id, phyId);
                var result = _dbContext.Database.SqlQuery<string>(query).FirstOrDefault();
                #region Method for reasigning of case
                //TeleSpecialists.Web.Controllers.DispatchController dispatchController = new Web.Controllers.DispatchController();
                //dispatchController.RejectCaseForApi(Id, null ,"from Api rejection");
                #endregion

                return Json(result);
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }

        #region Husnain Work On Api's
        [HttpGet]
        [Authorize]
        [Route("cases/list/RapidsMails")]
        [SwaggerResponse(HttpStatusCode.OK, "", typeof(RapidsEmailsResult))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]
        public IHttpActionResult RapidsMails(int pageSize = 50, int skip = 0)
        {
            try
            {
                var queryBuilder = new StringBuilder();
                queryBuilder.Append($"@Take={pageSize},@Skip={skip}");

                var list = _dbContext.Database.SqlQuery<RapidsDbModel>("Exec usp_api_rapids_mails " + queryBuilder.ToString()).ToList();
                var mailByType = list.Select(m => new RapidsEmails
                {
                    Id = m.rpd_key,
                    user_id = m.rpd_uid,
                    date =  m.rpd_date,
                    From =  m.rpd_from,
                    To =  m.rpd_to,
                    Subject = m.rpd_subject,
                    Body =  m.rpd_body,
                    Attachements =  m.rpd_attachments,
                    Attachement_html =  m.rpd_attachment_html,
                    Logs =  m.rpd_logs,
                    IsRead = m.rpd_is_read,
                    CreatedBy = m.rpd_created_by,
                    CreatedDate = m.rpd_created_date

                }).ToList();
                return Json(new RapidsEmailsResult { totalRecords = (list.Count() > 0 ? list.First().totalRecords : 0), cases = mailByType });
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }


        [HttpGet]
        [Authorize]
        [Route("cases/list/by-status")]
        [SwaggerResponse(HttpStatusCode.OK, "", typeof(CaseByPhysicianResult))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]
        public IHttpActionResult ByStatus( string SearchText = "", string caseStatus = "", string caseType = "", string phyId = "", string facId = "", DateTime? startDate = null, DateTime? endDate = null, int pageSize = 50, int skip = 0)
        {
            try
            {
                if (!string.IsNullOrEmpty(caseStatus))
                {
                    try
                    {
                        caseStatus = caseStatus.Replace(" ", "");
                    }
                    catch { }
                    caseStatus = caseStatus.ToLower();
                }
                    
                var queryBuilder = new StringBuilder();
                queryBuilder.Append($"@Take={pageSize},@Skip={skip}");
                if (!string.IsNullOrEmpty(phyId))
                    queryBuilder.Append(string.Format(",@Physician='{0}'", phyId));

                if (caseType != null)
                    queryBuilder.Append(string.Format(",@CaseTypeOpr='{0}'", caseType));

                if (startDate.HasValue)
                    queryBuilder.Append(string.Format(",@StartDate='{0}'", startDate.Value));

                if (endDate.HasValue)
                    queryBuilder.Append(string.Format(",@EndDate='{0}'", endDate.Value));
                
                #region Get Case Status Value
                int? _status = null;
                if (caseStatus == CaseStatus.Accepted.ToString().ToLower())
                {
                    _status = (int)CaseStatus.Accepted;
                }
                if (caseStatus == CaseStatus.Cancelled.ToString().ToLower())
                {
                    _status = (int)CaseStatus.Cancelled;
                }
                if (caseStatus == CaseStatus.Open.ToString().ToLower())
                {
                    _status = (int)CaseStatus.Open;
                }
                if (caseStatus == CaseStatus.Complete.ToString().ToLower())
                {
                    _status = (int)CaseStatus.Complete;
                }
                if (caseStatus == CaseStatus.WaitingToAccept.ToString().ToLower())
                {
                    _status = (int)CaseStatus.WaitingToAccept;
                }
                #endregion
                #region Get Case Type
                #endregion

                if (_status != null)
                    queryBuilder.Append(string.Format(",@caseStatus='{0}'", _status));

                if (!string.IsNullOrEmpty(facId))
                    queryBuilder.Append(string.Format(",@Facility='{0}'", facId));

                if (!string.IsNullOrEmpty(SearchText))
                    queryBuilder.Append(string.Format(",@SearchText='{0}'", SearchText));

                var list = _dbContext.Database.SqlQuery<CaseDbModel>("Exec usp_api_cases_by_phy_case_status " + queryBuilder.ToString()).ToList();
                var caseByType = list.Select(m => new CaseByPhysician
                {
                    Id = m.cas_key,
                    CaseNumber = m.cas_case_number,
                    CaseStatus = new KeyValType { Id = m.cas_cst_key.ToString(), Name = m.cst_name },
                    CaseType = new KeyValType { Id = m.cas_ctp_key.ToString(), Name = m.ctp_name },
                    Facility = new KeyValType { Id = m.cas_fac_key.ToString(), Name = m.fac_name },
                    Physician = new KeyValType { Id = m.cas_phy_key, Name = m.phy_name },
                    CreatedDate = m.cas_created_date

                }).ToList();
                return Json(new CaseByPhysicianResult { totalRecords = (list.Count() > 0 ? list.First().totalRecords : 0), cases = caseByType });
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }


        #endregion

        #region Firebase Api's By Husnain
        [Authorize]
        [HttpGet]
        [Route("case/sendtophysician")]
        [SwaggerResponse(HttpStatusCode.OK, "", typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]
        public IHttpActionResult SendToPhysician(int casekey)
        {
            try
            {
                user_fcm_notification user_Fcm_Notification = new user_fcm_notification();
                var queryBuilder = new StringBuilder();
                if (casekey != 0)
                    queryBuilder.Append(string.Format("@id='{0}'", casekey));
                var result = _dbContext.Database.SqlQuery<CaseCustomDetail>("Exec usp_api_get_case_with_id " + queryBuilder.ToString()).FirstOrDefault();
                if(result != null)
                {
                    List<string> list = new List<string>();
                    list.Add(result.cas_case_number.ToString());
                    list.Add(result.fac_name);
                    list.Add(result.cas_fac_key.ToString());
                    list.Add(result.PhysicianName);
                    var record = user_Fcm_Notification.GetobjectOfSendNotification(result.cas_phy_key, casekey, strokeDetail: list);
                    return Json( record);
                }


                return Json(result);
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("case/SaveToken")]
        [SwaggerResponse(HttpStatusCode.OK, "", typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]
        public IHttpActionResult SaveToken( string phyId, string token, string deviceType)
        {
            try
            {
                string query = string.Format("Exec  [dbo].[usp_api_case_accept] {0}, '{1}', '{2}'", phyId, token, deviceType);
                var result = _dbContext.Database.SqlQuery<string>(query).FirstOrDefault();


                return Json(result);
            }
            catch (Exception ex)
            {
                return ServerError(ex.Message);
            }
        }
        #endregion
    }
}