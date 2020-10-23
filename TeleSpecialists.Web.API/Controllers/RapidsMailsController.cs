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

namespace TeleSpecialists.Web.API.Controllers
{
    [Authorize]
    public class RapidsMailsController : BaseController
    {
        #region Husnain Work On Api's
        [HttpGet]
        [Authorize]
        [Route("RapidsMails/list/RapidsMails")]
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
                    date = m.rpd_date,
                    From = m.rpd_from,
                    To = m.rpd_to,
                    Subject = m.rpd_subject,
                    Body = m.rpd_body,
                    Attachements = m.rpd_attachments,
                    Attachement_html = m.rpd_attachment_html,
                    Logs = m.rpd_logs,
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

        [Authorize]
        [HttpPut]
        [Route("RapidsMails/read")]
        [SwaggerResponse(HttpStatusCode.OK, "", typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Bad Request", typeof(APIErrorResponse))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "Internal Server Error", typeof(APIErrorResponse))]
        public IHttpActionResult read(int Id)
        {
            try
            {
                var result = _dbContext.Database.ExecuteSqlCommand(string.Format("Exec  [dbo].[usp_api_update_rapidMails] @id='{0}'", Id));
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
