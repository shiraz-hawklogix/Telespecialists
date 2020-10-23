using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TeleSpecialists.BLL;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.Models;
using TeleSpecialists.Web.Hubs;
using TeleSpecialists.Web.Models;

namespace TeleSpecialists.Controllers
{
    public class BaseController : Controller
    {
        #region Global Variable
        //protected readonly UnitOfWork _unitOfWork;
        #endregion

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
        private ApplicationUser _loggedInUser;
        private AlarmService _alarmService = new AlarmService();

        private application_setting _appSettings;

        private bool _isTuneAllow = false;
         

        public BaseController()
        {            
        }


        //[OutputCache(Duration = 60, VaryByParam = "*", Location = System.Web.UI.OutputCacheLocation.ServerAndClient)]
        private ApplicationUser GetLoggedInUser(string userId)
        {
            return UserManager.Users.Where(x => x.Id == userId).FirstOrDefault();
        }

        private application_setting GetAppSettings()
        {
            using (AppSettingService _appSettingService = new AppSettingService())
            {
                return _appSettingService.Get();
            }
        }

        /// <summary>
        /// Update cache in case of update|create
        /// </summary>
        protected void RefreshApplicationSetting() { _appSettings = null; Web.Helpers.CacheHelper.Remove("TelecareAppSettings"); }

        //[OutputCache(Duration = 300, VaryByParam = "none")]
        protected application_setting ApplicationSetting
        {
            get
            {
                if (_appSettings == null)
                {
                    var getter = new Lazy<application_setting>(() => GetAppSettings());

                    _appSettings = Web.Helpers.CacheHelper.InsertAndGet("TelecareAppSettings", getter, "", 50);
                }
                return _appSettings;
            }
        }

        protected void RefreshLoggedInUser(string UserId = "") { _loggedInUser = null; Web.Helpers.CacheHelper.Remove(UserId == "" ? User.Identity.GetUserId() : UserId,"LoggedInId"); }
        public ApplicationUser loggedInUser
        {
            get
            {
                if (_loggedInUser == null)
                {
                    var userId = User.Identity.GetUserId();

                    if (string.IsNullOrEmpty(userId)) return null;

                    var getter = new Lazy<ApplicationUser>(() => GetLoggedInUser(userId));

                    _loggedInUser = Web.Helpers.CacheHelper.InsertAndGet(userId, getter, "LoggedInId");
                }

                return _loggedInUser;
            }
        }
        //added by husnain
        public bool isTuneAllow
        {
            get
            {
                _isTuneAllow = ApplicationSetting.aps_enable_alarm_setting;
                return _isTuneAllow;
            }
        }

        public List<string> OnlineUserIds
        {
            get
            {
                if (Settings.RPCMode == RPCMode.SignalR)
                {
                    return PhysicianCasePopupHub.ConnectedUsers.Select(m => m.UserId).Distinct().ToList();
                }
                else if (Settings.RPCMode == RPCMode.WebSocket)
                {
                    return new WebSocketEventHandler()._chatClients.Select(x => ((WebSocketEventHandler)x).UserId).Distinct().ToList();
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.loggedInUser = loggedInUser;
            ViewBag.checkTune = isTuneAllow;
            ViewBag.ApplicationSetting = ApplicationSetting;
            base.OnActionExecuting(filterContext);
        }

        #region Identity Management
        public IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        public ActionResult GetViewResult(object model = null)
        {
            if (Request.IsAjaxRequest())
            {
                return PartialView(model);
            }
            else
            {
                return View(model);
            }
        }
        public ActionResult GetViewResult(string partialViewName, object model = null)
        {
            if (Request.IsAjaxRequest())
            {
                return PartialView(partialViewName, model);
            }
            else
            {
                return View(partialViewName, model);
            }
        }
        public ActionResult GetErrorResult(object model, bool showValidationSummary = false)
        {
            if (Request.IsAjaxRequest())
            {
                var messageText = "";
                if (showValidationSummary)
                {
                    messageText += "<ul>";
                    messageText += string.Join("", this.GetModalErrors().Values.Select(m => "<li>" + m + "</li>"));
                    messageText += "</ul>";
                }
                else
                {
                    messageText = string.Join("<br/>", this.GetModalErrors().Values);
                }
                return Json(new AjaxResult { success = false, message = messageText });
            }
            else
            {
                return View(model);
            }
        }

        protected JsonResult JsonMax(object data, JsonRequestBehavior behavior)
        {
            var result = Json(data, behavior);
            result.MaxJsonLength = int.MaxValue;
            return result;
        }
        protected string RenderPartialViewToString(string viewName)
        {
            return RenderPartialViewToString(viewName, null);
        }
        protected string RenderPartialViewToString(string viewName, object model)
        {


            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
        public ActionResult GetSuccessResult(string redirectUrl = "", string message = "")
        {
            redirectUrl = string.IsNullOrEmpty(redirectUrl) ? Url.Action("Index") : redirectUrl;
            if (Request.IsAjaxRequest())
            {
                return Json(new AjaxResult { success = true, redirectUrl = redirectUrl, message = message });
            }
            else
            {
                return RedirectToAction(redirectUrl);
            }
        }
        public ActionResult ShowSuccessMessageOnly(string successMessage, object model)
        {
            if (Request.IsAjaxRequest())
            {
                return Json(new AjaxResult { success = true, message = successMessage, data = model });
            }
            else
            {
                return View(model);
            }
        }
        public ActionResult ShowErrorMessageOnly(string errorMessage, object model)
        {
            if (Request.IsAjaxRequest())
            {
                return Json(new AjaxResult { success = false, message = errorMessage, data = model });
            }
            else
            {
                return View(model);
            }
        }

        public void AddChangeLog(object changeSet, string entityKey, EntityTypes entityType)
        {

            using (EntityChangeLogService service = new EntityChangeLogService())
            {
                service.Create(new entity_change_log
                {
                    ecl_changeset = Newtonsoft.Json.JsonConvert.SerializeObject(changeSet),
                    ecl_created_by = System.Web.HttpContext.Current.User.Identity.GetUserId(),
                    ecl_created_by_name = loggedInUser.FullName,
                    ecl_created_date = DateTime.Now.ToEST(),
                    ecl_entity_key = entityKey,
                    ecl_type = entityType.ToDescription()
                });
            }
        }
        //public BaseController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager applicationRoleManager)
        //{
        //    UserManager = userManager;
        //    SignInManager = signInManager;
        //    RoleManager = applicationRoleManager;
        //}
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _userManager?.Dispose();
                _userManager = null;

                _signInManager?.Dispose();
                _signInManager = null;

                _roleManager?.Dispose();
                _roleManager = null;

                //_unitOfWork?.Dispose();
            }

            base.Dispose(disposing);
        }
        public string CheckPasswordResetValidation(application_setting setting, string passValue)
        {
            string errorMessage = "";
            if (setting.aps_security_is_lowercase_required || setting.aps_security_is_uppercase_required || setting.aps_security_is_number_required || setting.aps_security_is_non_alphanumeric_required || setting.aps_security_password_length_value != null)
            {
                bool isLowerCharacter = passValue.Any(char.IsLower);
                bool isUpperCharacter = passValue.Any(char.IsUpper);
                bool isNumber = passValue.Any(char.IsNumber);
                bool isSpecialChar = passValue.Any(c => !char.IsLetterOrDigit(c));
                int length = passValue.Length;

                if (length < setting.aps_security_password_length_value || length > 16)
                    errorMessage = "Password must be " + setting.aps_security_password_length_value + " to 16 characters and contain:";

                if (errorMessage == "")
                    errorMessage = "Password required at least";
                if (setting.aps_security_is_lowercase_required && !isLowerCharacter)
                    errorMessage += " one Lower Case (a-z),";
                if (setting.aps_security_is_uppercase_required && !isUpperCharacter)
                    errorMessage += " one Upper Case (A-Z),";
                if (setting.aps_security_is_number_required && !isNumber)
                    errorMessage += " one Numeric (0-9),";
                if (setting.aps_security_is_non_alphanumeric_required && !isSpecialChar)
                {
                    if (errorMessage == "Password required at least")
                        errorMessage += " one Non-Alphanumeric (!,@,#,$,%,^,*,_,+,=).";
                    else
                        errorMessage += " and one Non-Alphanumeric (!,@,#,$,%,^,*,_,+,=).";
                }
                if (errorMessage != "Password required at least")
                {
                    errorMessage.TrimEnd(',');
                    return errorMessage;
                }


            }
            return "";
        }
        public bool CheckPasswordValidation(application_setting setting, string passValue)
        {
            bool isError = false;
            if (setting.aps_security_is_lowercase_required || setting.aps_security_is_uppercase_required || setting.aps_security_is_number_required || setting.aps_security_is_non_alphanumeric_required || setting.aps_security_password_length_value != null)
            {
                bool isLowerCharacter = passValue.Any(char.IsLower);
                bool isUpperCharacter = passValue.Any(char.IsUpper);
                bool isNumber = passValue.Any(char.IsNumber);
                bool isSpecialChar = passValue.Any(c => !char.IsLetterOrDigit(c));
                int length = passValue.Length;

                if ((setting.aps_security_is_lowercase_required && !isLowerCharacter) || (setting.aps_security_is_uppercase_required && !isUpperCharacter) || (setting.aps_security_is_number_required && !isNumber) || (setting.aps_security_is_non_alphanumeric_required && !isSpecialChar) || (setting.aps_security_password_length_value != null && length < setting.aps_security_password_length_value) || (setting.aps_security_password_length_value != null && length > 16))
                {
                    isError = true;
                    return isError;
                }
            }
            return isError;
        }
        public bool CheckPasswordErrors(application_setting setting, string password)
        {
            string errorMessage = "";
            bool isError = false;
            if (setting.aps_security_is_lowercase_required || setting.aps_security_is_uppercase_required || setting.aps_security_is_number_required || setting.aps_security_is_non_alphanumeric_required || setting.aps_security_password_length_value != null)
            {
                bool isLowerCharacter = password.Any(char.IsLower);
                bool isUpperCharacter = password.Any(char.IsUpper);
                bool isNumber = password.Any(char.IsNumber);
                bool isSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));
                int length = password.Length;

                if (length < setting.aps_security_password_length_value || length > 16)
                {
                    isError = true;
                    errorMessage = "Password must be " + setting.aps_security_password_length_value + " to 16 characters and contain:";
                    ModelState.AddModelError("password_length", "Password must be " + setting.aps_security_password_length_value + " to 16 characters and contain:");
                }

                if (isError == false && ((setting.aps_security_is_lowercase_required && !isLowerCharacter) || (setting.aps_security_is_uppercase_required && !isUpperCharacter) || (setting.aps_security_is_number_required && !isNumber) || (setting.aps_security_is_non_alphanumeric_required && !isSpecialChar)))
                {
                    errorMessage = "Password required at least";
                    ModelState.AddModelError("Password", "Password required at least:");
                }

                if (setting.aps_security_is_lowercase_required && !isLowerCharacter)
                {
                    isError = true;
                    errorMessage += "one Lower Case (a-z),";
                    ModelState.AddModelError("lowercase", "1 Lower Case Letter");
                }
                if (setting.aps_security_is_uppercase_required && !isUpperCharacter)
                {
                    isError = true;
                    errorMessage += " one Upper Case (A-Z),";
                    ModelState.AddModelError("uppercase", "1 Capital Letter");
                }
                if (setting.aps_security_is_number_required && !isNumber)
                {
                    isError = true;
                    errorMessage = "one Numeric (0-9),";
                    ModelState.AddModelError("Numeric", "1 Number");
                }
                if (setting.aps_security_is_non_alphanumeric_required && !isSpecialChar)
                {
                    isError = true;
                    errorMessage += " one Non-Alphanumeric (!,@,#,$,%,^,&,*,_,+,=).";
                    ModelState.AddModelError("Non-Alphanumeric", "1 Special Character(!,@,#,$,%,^,&,*,_,+,=).");
                }

                if (errorMessage != "Password required at least")
                    return isError;
            }
            return isError;
        }


        #region TCARE-581 
        protected void LogAuditRecord(string username, string action)
        {
            try
            {
                var ipaddress = HttpContext.Request.UserHostAddress;
                var hostname = HttpContext.Request.UserHostName;
                var userlanguages = HttpContext.Request.UserLanguages;
                var browser = HttpContext.Request.Browser.Browser;

                var record = new audit_records()
                {
                    aud_username = username,
                    aud_action = action,
                    aud_browser = browser,
                    aud_host_name = hostname,
                    aud_ip_address = ipaddress,
                    aud_timestamp = DateTime.Now.ToEST()
                }; 

                using (var _auditRecordsService = new AuditRecordsService())
                {
                    _auditRecordsService.Create(record);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }
        #endregion
    }
}