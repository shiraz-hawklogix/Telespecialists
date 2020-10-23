using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Web.Routing;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace System.Web.Mvc
{
    public class AccessRoles : AuthorizeAttribute
    {
        private string _notInRoles;
        private List<string> _notInRolesList;
        public string NotInRoles
        {
            get
            {
                return _notInRoles ?? string.Empty;
            }
            set
            {
                _notInRoles = value;
                if (!string.IsNullOrWhiteSpace(_notInRoles))
                {
                    _notInRolesList = _notInRoles
                        .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(r => r.Trim()).ToList();
                }
            }
        }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (HttpContext.Current.User == null)
            {

                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.HttpContext.Response.StatusCode = 401;
                    filterContext.HttpContext.Response.End();
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", "Login" }, { "controller", "Account" } });
                }
            }
            else
            {
                //if (filterContext.HttpContext.User.IsInRole("Super Admin")) { }
                //else if (filterContext.HttpContext.User.IsInRole("Domain Admin")) { }

                bool isAllowed = false;

                // get user roles
                var userRoles = filterContext.HttpContext.GetOwinContext().GetUserManager<TeleSpecialists.ApplicationUserManager>().GetRoles(filterContext.HttpContext.User.Identity.GetUserId()) as List<string>;

                // check if user assigned role has access to this page
                if (this.Roles.Split(',').Length > 0)
                {
                    foreach (string role in this.Roles.Split(','))
                    {
                        if (userRoles.Contains(role.Trim())) isAllowed = true;
                    }
                }
                else if (userRoles.Contains(this.Roles))
                {
                    // do nothing, its in allowed roles
                    isAllowed = true;
                }

                if (_notInRolesList != null && _notInRolesList.Count > 0)
                {
                    isAllowed = true;
                    foreach (var restrictedRole in _notInRolesList)
                    {
                        if (userRoles.Contains(restrictedRole.Trim()))
                        {
                            isAllowed = false;
                        }
                    }
                }

                if (!isAllowed)
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", "AccessDenied" }, { "controller", "Account" } });
                }
            }
        }
    }
    public class AuthenticateUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User == null)
            {
                //if (filterContext.HttpContext.Session["userInfo"] == null)
                filterContext.Result = new RedirectResult("/Account/Login");
            }
        }
    }
    public class AuthenticateSignalRequestAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.Params["SignalRAuthKey"] != null)
            {
               
                if (filterContext.HttpContext.Request.Params["SignalRAuthKey"] != TeleSpecialists.BLL.Settings.SignalRAuthKey)                    
                    filterContext.Result = new RedirectResult("/Account/Login");
            }
            else
            {
                filterContext.Result = new RedirectResult("/Account/Login");
            }
        }
    }
    public class ValidateFileAttribute : ComponentModel.DataAnnotations.ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null) { return true; }
            int MaxContentLength = 1024 * 1024 * 3; //3 MB
            Collections.Generic.List<string> AllowedFileExtensions = new Collections.Generic.List<string> { ".jpg", ".gif", ".png" };

            var file = value as HttpPostedFileBase;

            if (file == null)
                return false;

            else if (!AllowedFileExtensions.Contains(file.FileName.ToLower().Substring(file.FileName.LastIndexOf('.'))))
            {
                ErrorMessage = "Accepts jpg, .png, .gif up to 3MB.";// + string.Join(", ", AllowedFileExtensions);
                return false;
            }
            else if (file.ContentLength > MaxContentLength)
            {
                ErrorMessage = "Your Photo is too large, maximum allowed size is : " + (MaxContentLength / 1024).ToString() + "MB";
                return false;
            }
            else
                return true;
        }
    }

    public class RequiredAttributeAdapter : DataAnnotationsModelValidator<RequiredAttribute>
    {
        public RequiredAttributeAdapter(ModelMetadata metadata, ControllerContext context, RequiredAttribute attribute)
            : base(metadata, context, attribute)
        {
        }
        public static void SelfRegister()
        {
            DataAnnotationsModelValidatorProvider
                .RegisterAdapter(
                    typeof(RequiredAttribute),
                    typeof(RequiredAttributeAdapter));
        }
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return new[] { new ModelClientValidationRequiredRule("This field is required.") };
        }
    }
    //public class DeleteFileAttribute : ActionFilterAttribute
    //{
    //    public override void OnResultExecuted(ResultExecutedContext filterContext)
    //    {
    //        try
    //        {
    //            filterContext.HttpContext.Response.Flush();
    //            var vBagFullPath = filterContext.Controller.ViewBag.TempFilePath;
    //            if (vBagFullPath != null)
    //            {
    //                System.IO.File.Delete(vBagFullPath);
    //            }
    //        }
    //        catch
    //        {

    //        }
           
    //    }
    //}

}