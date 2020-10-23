using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using TeleSpecialists.Models;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;

namespace TeleSpecialists.Controllers
{
    [Authorize]
    public class ManageController : BaseController
    {
        
        private ApplicationUserManager _userManager;
        private readonly AspNetUsersLogService _userLogService;

        public ManageController()
        {
            _userLogService = new AspNetUsersLogService();
        }
                 
         
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                LogAuditRecord(User.Identity.Name, AuditRecordLogStatus.RemoveLogin.ToDescription());
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            LogAuditRecord(User.Identity.Name, AuditRecordLogStatus.TwoFactorEnabled.ToDescription());
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            LogAuditRecord(User.Identity.Name, AuditRecordLogStatus.TwoFactorDisabled.ToDescription());

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                LogAuditRecord(User.Identity.Name, AuditRecordLogStatus.PhoneVrified.ToDescription());
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false); 
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            LogAuditRecord(User.Identity.Name, AuditRecordLogStatus.PhoneRemoved.ToDescription());
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);      
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return GetViewResult();
        }
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return GetErrorResult(model);
            }

            bool errorMsg = CheckPasswordValidation(ApplicationSetting, model.NewPassword);
            if (errorMsg == true)
            {
                ModelState.AddModelError("a", "Password must be " + ApplicationSetting.aps_security_password_length_value + " to 16 characters and contain:");
                ModelState.AddModelError("b", "1 Capital Letter");
                ModelState.AddModelError("c", "1 Lower Case Letter");
                ModelState.AddModelError("d", "1 Number");
                ModelState.AddModelError("e", "1 Special Character(!,@,#,$,%,^,*,_,+,=)");
                return GetErrorResult(model, true);
            }

            var userForPassUpdate = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var hashOldPassword = userForPassUpdate.PasswordHash;
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Errors.Count() > 0 && ((string[])result.Errors)[0] == "Incorrect password.")
            {
                ModelState.AddModelError("Password", ((string[])result.Errors)[0]);
                return GetErrorResult(model);
            }
            else
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                var hashNewPassword = user.PasswordHash;
                if (user != null)
                {
                    // This is for password Age
                    if (ApplicationSetting.aps_security_password_age_value > 0)
                    {
                        user.PasswordExpirationDate = DateTime.UtcNow.ToEST().AddDays(ApplicationSetting.aps_security_password_age_value);
                    }
                    else
                        user.PasswordExpirationDate = null;

                    // This is for password history
                    if (ApplicationSetting.aps_security_password_history_value.HasValue)
                    {
                        var userPasswordLog = _userLogService.GetUserLog(user.Id, (int)ApplicationSetting.aps_security_password_history_value);
                        foreach (var item in userPasswordLog)
                        {
                            var passResult = UserManager.PasswordHasher.VerifyHashedPassword(item.PasswordHash_New, model.NewPassword);
                            switch (passResult)
                            {
                                case PasswordVerificationResult.Success:
                                    ModelState.AddModelError("Password", "You cannot use your previous " + ApplicationSetting.aps_security_password_history_value + " passwords.");
                                    return GetErrorResult(model);
                            }
                        }
                    }

                    var objUserLog = new AspNetUsers_Log
                    {
                        PasswordHash_Old = hashOldPassword,
                        SecurityStamp_Old = user.SecurityStamp,
                        PasswordHash_New = hashNewPassword,
                        CreatedBy = User.Identity.GetUserId(),
                        CreatedDate = DateTime.Now.ToEST(),
                        CreatedByName = loggedInUser.FullName,
                        ModifiedBy = User.Identity.GetUserId(),
                        ModifiedDate = DateTime.Now.ToEST(),
                        ModifiedByName = loggedInUser.FullName,
                        AspNetUsersId = loggedInUser.Id
                    };

                    _userLogService.Create(objUserLog);

                    user.PasswordHash = hashNewPassword;
                    //After change the password the user must need to reset the password if it is set to true from security
                    user.RequirePasswordReset = true;
                    user.ModifiedBy = user.Id;
                    user.ModifiedByName = user.FullName;
                    user.ModifiedDate = DateTime.Now.ToEST();

                    if (string.IsNullOrEmpty(user.SecurityStamp))
                    {
                        user.SecurityStamp = Guid.NewGuid().ToString();
                    }

                    var resultUser = await UserManager.UpdateAsync(user);
                    if (!resultUser.Succeeded)
                    {
                        ModelState.AddModelError("", resultUser.Errors.First());
                        return View(model);
                    }

                    LogAuditRecord(user.UserName, AuditRecordLogStatus.PasswordChange.ToDescription());
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return GetSuccessResult(Url.Action("Index", "Home"));
            }
        }
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    LogAuditRecord(User.Identity.Name, AuditRecordLogStatus.SetPassword.ToDescription());
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _userLogService?.Dispose();
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";
        
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }
        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }
        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }
        #endregion
    }
}