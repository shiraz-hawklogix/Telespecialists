using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using TeleSpecialists.Web.API.Extensions;
using TeleSpecialists.Web.API.Models;
using Microsoft.AspNet.Identity.Owin;

namespace TeleSpecialists.Web.API.Controllers
{
    public class BaseController : ApiController
    {
        protected readonly TelecareDbContext _dbContext;
        private UserManager<ApplicationUser> _userManager;

        public UserManager<ApplicationUser> UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<UserManager<ApplicationUser>>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public BaseController()
        {
            _dbContext = new TelecareDbContext();
            _dbContext.Database.CommandTimeout = int.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("DBConnectionTimeout"));
        }

        protected IHttpActionResult MissingStartDate()
        {
            return Content(HttpStatusCode.BadRequest, new APIErrorResponse { ErrorCode = HttpStatusCode.BadRequest, ErrorMessage = "Start Date is not valid." });
        }
        protected IHttpActionResult MissingEndtDate()
        {
            return Content(HttpStatusCode.BadRequest, new APIErrorResponse { ErrorCode = HttpStatusCode.BadRequest, ErrorMessage = "End Date is not valid." });
        }
        protected IHttpActionResult ServerError(string message)
        {
            return Content(HttpStatusCode.InternalServerError, new APIErrorResponse { ErrorCode = HttpStatusCode.InternalServerError, ErrorMessage = message });
        }

        protected IHttpActionResult UnAuthorizedRequest(string message)
        {
            return Content(HttpStatusCode.Unauthorized, new APIErrorResponse { ErrorCode = HttpStatusCode.Unauthorized, ErrorMessage = message });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}