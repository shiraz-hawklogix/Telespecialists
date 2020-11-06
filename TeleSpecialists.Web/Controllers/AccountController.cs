using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.ViewModels;
using TeleSpecialists.Models;
using TeleSpecialists.Web.Hubs;
using TeleSpecialists.Web.Models;
using Twilio;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace TeleSpecialists.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private readonly AspNetUsersLogService _userLogService;
        private readonly AspNetUsersPasswordResetService _userPassResetService;
        private readonly AlarmService _alarmService;
        private readonly UserVerificationService _userVerificationService;
        private readonly user_fcm_notification _user_Fcm_Notification;
        private readonly TokenService _tokenservice;
        private readonly MenuService _menuService;
        //   private readonly RateService _rateService;
        public AccountController()
        {
            _userLogService = new AspNetUsersLogService();
            _userPassResetService = new AspNetUsersPasswordResetService();
            _alarmService = new AlarmService();
            _userVerificationService = new UserVerificationService();
            _user_Fcm_Notification = new user_fcm_notification();
            _tokenservice = new TokenService();
            _menuService = new MenuService();
            //     _rateService = new RateService();
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl, bool isPasswordReset = false)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (isPasswordReset)
                ViewBag.passwordChanged = "Your password has successfully been changed.";
            //var userName = TempData["passwordHasbeenChanged"] as string;
            //ViewBag.passwordChanged = userName;

            ViewBag.login = "Login";
            return View();
        }
        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.login = "Login";
                    return View(model);
                }
                var user = await UserManager.FindAsync(model.Username, model.Password);

                if (user != null)
                {
                    var IsApiUser = UserManager.GetRoles(user.Id).Contains(UserRoles.TeleCareApi.ToDescription());
                    if (user.IsActive && user.IsDisable == false && user.IsDeleted == false && !IsApiUser)
                    {
                        //commenting this code for temporary basis of build
                        //var rolesaccess = _menuService.getMenuAccess(user.Roles.Select(x => x.RoleId).FirstOrDefault());
                        //Session["RoleAccess"] = rolesaccess;
                        //If Password was changed by admin OR user is going to login first time
                        if (ApplicationSetting.aps_secuirty_is_reset_password_required && !user.RequirePasswordReset)
                        {
                            ViewBag.UserName = model.Username;
                            ViewBag.IsPasswordExpired = false;
                            return View("ChangePasswordOnFirstLogin");
                        }
                        // This is for if password age was reset by admin
                        if (user.PasswordExpirationDate != null)
                        {
                            var expirationDate = Convert.ToDateTime(user.PasswordExpirationDate);
                            var currentDate = DateTime.UtcNow.ToEST();
                            if (expirationDate < currentDate)
                            {
                                ViewBag.UserName = model.Username;
                                ViewBag.IsPasswordExpired = true;
                                return View("ChangePasswordOnFirstLogin");
                            }
                        }

                        #region [Check Two Factor Authentication]
                        var verificationStatus = UserVerification(user.TwoFactorEnabled, user.Id, model.isAuthenticationChecked,model.isLogout);
                        switch (verificationStatus)
                        {
                            case UserLoggedInVerificationStatus.LogOut:
                                model.UserId = user.Id;
                                ViewBag.login = "LogoutMachines";
                                return View(model);

                            case UserLoggedInVerificationStatus.IsAuthentationDisabled:
                                user_login_verify _model = new user_login_verify();
                                _model.UserId = user.Id;
                                _model.MachineIpAddress = UserIPAddress();
                                _model.MachineName = GetUniqueMachineInfo(_model.UserId);
                                _userVerificationService.addVerificationEntry(_model);
                                break;

                            case UserLoggedInVerificationStatus.IsAuthentationEnabled:
                                model.UserId = user.Id;
                                string phoneNumber = Regex.Replace(user.MobilePhone, @"[^0-9a-zA-Z]+", "").Trim();
                                phoneNumber = Regex.Replace(phoneNumber.Substring(0, 6), "[0-9]", "*") + phoneNumber.Substring(6, 4);
                                model.PhoneNumber = "**" + phoneNumber;
                                model.Email = user.Email;
                                ViewBag.login = "CodeVerify";
                                return View(model);
                            default:
                                break;
                        }

                        #endregion     

                        // This doesn't count login failures towards account lockout
                        // To enable password failures to trigger account lockout, change to shouldLockout: true
                        var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, shouldLockout: true);
                        switch (result)
                        {
                            case SignInStatus.Success:

                                LogAuditRecord(user.UserName, AuditRecordLogStatus.LoginSucess.ToDescription());
                                this.RefreshLoggedInUser(user.Id);
                                if (user.RequirePasswordReset == false)
                                {
                                    user.RequirePasswordReset = true;
                                    var resultUpdate = await UserManager.UpdateAsync(user);
                                }
                                return RedirectToLocal("");
                            case SignInStatus.LockedOut:
                                ////Sending Reset Password link 
                                ///
                                LogAuditRecord(user.UserName, AuditRecordLogStatus.AccountLockOut.ToDescription());

                                SendPasswordResetLink(user);
                                ModelState.AddModelError("", "You have exceeded the limit of login attempts. Please check your email to reset your password.");
                                ViewBag.login = "Login";
                                return View(model);
                            case SignInStatus.RequiresVerification:
                                LogAuditRecord(user.UserName, AuditRecordLogStatus.RequireVarification.ToDescription());

                                return RedirectToAction("SendCode", new { RememberMe = model.RememberMe });
                            case SignInStatus.Failure:
                            default:
                                LogAuditRecord(user.UserName, AuditRecordLogStatus.LoginFailed.ToDescription() );

                                await UserManager.AccessFailedAsync(user.Id);
                                ModelState.AddModelError("", "Invalid login attempt");
                                ViewBag.login = "Login";
                                return View(model);
                        }

                    }
                }
                else
                {
                    //If it comes here, it means that user has invalid username or password. 

                    user = UserManager.FindByName(model.Username);

                    if (user != null)
                    {
                        if (user.AccessFailedCount >= UserManager.MaxFailedAccessAttemptsBeforeLockout-1)
                        {
                            /// AccessFailedCount >= UserManager.MaxFailedAccessAttemptsBeforeLockout-1 means user failed 4 time before and this is its last or 5th time.
                            /// So here we will lock the user. 

                            if (!user.LockoutEnabled)
                            {
                                await UserManager.SetLockoutEnabledAsync(user.Id, true);

                                //Locking for 200 years is enough I guess.
                                var lockoutTill = DateTime.UtcNow.ToEST().AddYears(200);
                                await UserManager.SetLockoutEndDateAsync(user.Id, lockoutTill);
                            }

                            ////Sending Reset Password link 
                            SendPasswordResetLink(user);

                            ModelState.AddModelError("", "You have exceeded the limit of login attempts. Please check your email to reset your password.");
                            LogAuditRecord(user.UserName, AuditRecordLogStatus.LoginAttemptsExceeded.ToDescription() );
                            ViewBag.login = "Login";
                            return View(model);
                        }
                        else
                        {
                            //Incrementing AccessFailedCount.
                            await  UserManager.AccessFailedAsync(user.Id);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            ModelState.AddModelError("", "Invalid login attempt");
            ViewBag.login = "Login";
            return View(model);
        }


        #region Two Factor Authentication by Bilal

        #region get Client Public Ip
        private static long ConvertIPToLong(string ip)
        {
            string[] ipSplit = ip.Split('.');
            return (16777216 * Convert.ToInt32(ipSplit[0]) + 65536 * Convert.ToInt32(ipSplit[1]) + 256 * Convert.ToInt32(ipSplit[2]) + Convert.ToInt32(ipSplit[3]));
        }

        private bool TryConvertIPToLong(string ip, out long ipToLong)
        {
            try
            {
                ipToLong = ConvertIPToLong(ip);
                return true;
            }
            catch
            {
                ipToLong = -1;
                return false;
            }
        }

        private bool CheckIP(string ip)
        {
            string[] IP_Start = ConfigurationManager.AppSettings["PRIVATE_IP_RANGE_START"].Split(',');
            string[] IP_End = ConfigurationManager.AppSettings["PRIVATE_IP_RANGE_END"].Split(',');

            if (!String.IsNullOrEmpty(ip))
            {
                long ipToLong = -1;
                //Is it valid IP address
                if (TryConvertIPToLong(ip, out ipToLong))
                {
                    //Does it fall within a private network range
                    for (int i = 0; i < IP_Start.Length; i++)
                        if ((ipToLong >= ConvertIPToLong(IP_Start[i])) && (ipToLong <= ConvertIPToLong(IP_End[i])))
                            return false;
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public string UserIPAddress()
        {
            string Public_IP_Address = string.Empty;
            try
            {
                string HTTP_X_FORWARDED_FOR = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!string.IsNullOrEmpty(HTTP_X_FORWARDED_FOR))
                {
                    foreach (string ip in HTTP_X_FORWARDED_FOR.Split(','))
                    {
                        if (CheckIP(ip.Trim()))
                        {
                            //Use this IP address for authentication
                            Public_IP_Address = Public_IP_Address + ip.Trim() + ", ";
                        }
                    }
                    if (Public_IP_Address.Length > 0 && Public_IP_Address.EndsWith(", "))
                    {
                        Public_IP_Address = Public_IP_Address.Substring(0, Public_IP_Address.Length - 2);
                    }
                }

                if (string.IsNullOrEmpty(Public_IP_Address))
                {
                    Public_IP_Address = Request.ServerVariables["REMOTE_ADDR"];
                }

                return Public_IP_Address;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return "";
            }

        }

        #endregion

        public string GetUniqueMachineInfo(string UserId)
        {
            string PCName = "";
            HttpCookie myCookie = Request.Cookies["PCCookieInfo_" + UserId];
            if (myCookie != null)
            {
                PCName = myCookie.Values["userid"].ToString();
            }

            return PCName;
        }

        [HttpGet]
        public ActionResult CheckLogoutUser()
        {
            var userId = User.Identity.GetUserId();
            string pcName = GetUniqueMachineInfo(userId);
            if (userId != null)
            {
                bool result = _userVerificationService.CheckUserLogOut(userId.ToString(), pcName);
                return Json(new { Status = result }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Status = false }, JsonRequestBehavior.AllowGet);
        }

        private string SendSmsCode(LoginViewModel model, string PhoneNumber)
        {
            try
            {
                var smsVerificationCode = GenerateSimpleSmsVerificationCode();
                string accountSid = ConfigurationManager.AppSettings["AccountSid"].ToString();
                string authToken = ConfigurationManager.AppSettings["AuthKey"].ToString();
                string FromPhone = ConfigurationManager.AppSettings["FromPhone"].ToString();

                //PhoneNumber = ConfigurationManager.AppSettings["ToPhone"].ToString();
                //string ToPhone = "+92" + Regex.Replace(PhoneNumber, @"[^0-9a-zA-Z]+", "").Trim();

                string ToPhone = "+1" +Regex.Replace(PhoneNumber, @"[^0-9a-zA-Z]+", "").Trim();

                TwilioClient.Init(accountSid, authToken);
                var restClient = new TwilioRestClient(accountSid, authToken);

                var message = MessageResource.Create(
                    to: new PhoneNumber(ToPhone),
                    from: new PhoneNumber(FromPhone),
                    body: string.Format(
                                    "Telecare " +
                                    "login pin code is: {0}",
                                    smsVerificationCode)
                                    );

                return smsVerificationCode;

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return "";
            }
        }

        private string SendEmailCode(LoginViewModel model, string Email)
        {
            try
            {
                var emailVerificationCode = GenerateSimpleSmsVerificationCode();
                string EmailTo = Email;
                string Subject = ConfigurationManager.AppSettings["TwoFactorEmailSubject"].ToString();
                string body = "Telecare Login pin code is : " + emailVerificationCode;
                EmailHelper.SendTwoFactorAuthEmail(EmailTo, Subject, body);
                return emailVerificationCode;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return "";
            }
        }      

        private static string GenerateSimpleSmsVerificationCode()
        {
            string chars = ConfigurationManager.AppSettings["TwoFactorVerificationCode"].ToString();
            var random = new Random();
            return new string(
            Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)])
            .ToArray());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> CodeVerify(LoginViewModel model)
        {
            if (string.IsNullOrEmpty(model.TwoFactVerifyCode))
            {
                return Json(new { Status = false, Message = "Pin code is required." }, JsonRequestBehavior.AllowGet);
            }
            var user = await UserManager.FindAsync(model.Username, model.Password);

            if (user.TwoFactVerifyCode == model.TwoFactVerifyCode)
            {
                if (DateTime.UtcNow.ToEST() > user.CodeExpiryTime)
                {
                    return Json(new { Status = false, Message = "Pin code time has been expired." }, JsonRequestBehavior.AllowGet);
                }
                user_login_verify _model = new user_login_verify();
                _model.IsTwoFactRememberChecked = model.RememberMeSms;
                _model.TwoFactVerifyExpiryDate = model.RememberMeSms == true ? DateTime.Now.AddDays(30) : DateTime.Now;
                _model.IsLoggedIn = true;
                _model.MachineName = GetUniqueMachineInfo(model.UserId);
                _model.MachineIpAddress = UserIPAddress();
                _model.UserId = user.Id.ToString();
                _userVerificationService.addVerificationEntry(_model);

                return Json(new { Status = true, Message = "" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Status = false, Message = "Pin code is invalid." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult sendTwoFactCode(string Id, string verificationMethod = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(Id) && !(string.IsNullOrEmpty(verificationMethod)))
                {
                    var user = UserManager.FindById(Id);
                    LoginViewModel model = new LoginViewModel();
                    string TwoFactorCode = "";
                    string NotifyMessage = "";
                    if (verificationMethod.ToLower() == "sms")
                    {
                        TwoFactorCode = SendSmsCode(model, user.MobilePhone);
                        NotifyMessage = "Pin code has been sent to your mobile.";
                    }
                    else
                    {
                        TwoFactorCode = SendEmailCode(model, user.Email);
                        NotifyMessage = "Pin code has been sent to your email.";
                    }

                    if (!string.IsNullOrEmpty(TwoFactorCode))
                    {
                        user.TwoFactVerifyCode = TwoFactorCode;
                        var settings = ViewBag.ApplicationSetting as application_setting;
                        user.CodeExpiryTime = DateTime.UtcNow.ToEST().AddMinutes(Convert.ToDouble(settings.aps_two_factor_code_expiry_timeout_in_minutes));
                        var resultUpdate = UserManager.Update(user);
                        return Json(new { Status = true, Message = NotifyMessage }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { Status = false, Message = "Pin code is not generated. Please try again." }, JsonRequestBehavior.AllowGet);
                    }

                }
                return RedirectToAction("login");
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { Status = false, Message = "Some error occure. Please try again." }, JsonRequestBehavior.AllowGet);
            }
        }

        public UserLoggedInVerificationStatus UserVerification(bool TwoFactorEnabled, string UserId, string isAuthenticationChecked,string isLogout)
        {
            //check wether user is already loggedIn or any other macine or not
            var settings = ViewBag.ApplicationSetting as application_setting;
            HttpCookie myCookie = Request.Cookies["PCCookieInfo_" + UserId];
            if (myCookie == null)
            {
                myCookie = new HttpCookie("PCCookieInfo_" + UserId);
                myCookie.Values.Add("userid", Guid.NewGuid().ToString());
                myCookie.Expires = DateTime.Now.AddYears(2000);
                myCookie.HttpOnly = true;
                myCookie.Secure = true;
                Response.Cookies.Add(myCookie);
            }
            string PCName = GetUniqueMachineInfo(UserId);

            var user =  _userVerificationService.userVerifications(UserId);
            if (settings.aps_enable_logout_from_other_devices && user.Count > 0 && !string.IsNullOrEmpty(PCName) && (string.IsNullOrEmpty(isLogout) || isLogout != "true"))
            {
                if (user.Where(x => x.IsLoggedIn == true).ToList().Any(x => x.MachineName != PCName))
                {
                    return UserLoggedInVerificationStatus.LogOut;
                }
            }


            #region two factor authentication
           
            if (!settings.aps_secuirty_is_multi_factor_authentication_required)
            {
                return UserLoggedInVerificationStatus.IsAuthentationDisabled;
            }
            if (!User.IsInRole(UserRoles.FacilityNavigator.ToString()) && TwoFactorEnabled && (string.IsNullOrEmpty(isAuthenticationChecked) || isAuthenticationChecked != "true"))
            {
                if (user.Where(x => x.MachineName == PCName).Any(x => x.IsTwoFactRememberChecked == true) && DateTime.Now <= user.Where(x => x.MachineName == PCName).Select(x => x.TwoFactVerifyExpiryDate).FirstOrDefault() && !string.IsNullOrEmpty(PCName))
                {
                    return UserLoggedInVerificationStatus.IsAuthentationDisabled;
                }
                else
                {
                    return UserLoggedInVerificationStatus.IsAuthentationEnabled;
                }
            }
            else
            {
                return UserLoggedInVerificationStatus.IsAuthentationDisabled;
            }
            #endregion            
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult LogOutOtherLoggedInUser(LoginViewModel model)
        {
            if (string.IsNullOrEmpty(model.UserId))
            {
                return RedirectToAction("Login");
            }
            #region firebase
            string PCName = GetUniqueMachineInfo(model.UserId);
            var firebaseUsers = _userVerificationService.LogoutUserList(model.UserId, PCName);
            var phy_ids = firebaseUsers.ToList();
            var paramData = new List<object>();
            paramData.Add(JsonConvert.SerializeObject(phy_ids));
           // bool sentStatus = true;
            bool sentStatus = _user_Fcm_Notification.SendNotification(phy_key: model.UserId, caseType: "TwoFactorAuth", Data: paramData);
            if (sentStatus || !sentStatus)
            {
                _userVerificationService.SignOutAllUsers(model.UserId);
                return Json(new { result = true }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetMachineName()
        {
            string UserId = loggedInUser.Id + "_" + GetUniqueMachineInfo(loggedInUser.Id.ToString());
            return Json(new { result = UserId }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult getSessionTimeOutValue()
        {
            var UserId = ViewBag.ApplicationSetting as application_setting;

            return Json(new { result = UserId.aps_session_timeout_in_minutes }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }
        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }
        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }
        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }
        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }
        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await UserManager.FindByNameAsync(model.Username);
                    //var user = await UserManager.FindByEmailAsync(model.Email);
                    //if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                    if (user == null)
                        ViewBag.Error = "Invalid Username. Please enter valid Username OR contact system administrator for more details.";
                    else if (user.Email != model.Email)
                        ViewBag.Error = "No email address against the username exist in the system. Please enter valid email";
                    else
                    {
                        SendPasswordResetLink(user);

                        return RedirectToAction("ForgotPasswordConfirmation", "Account");
                    }
                }
                // If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
            }
            return View(model);
        }
        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string userId, string code)
        {
            var userPassDetail = _userPassResetService.GetUserDetails(userId);
            if (userPassDetail == null)
            {
                ViewBag.Error = "User not exist.";
            }
            else
            {
                var passwordCode = userPassDetail.Code;
                if (passwordCode != code)
                    ViewBag.Error = "Invalid code. Please send a valid code to reset password.";
                if (userPassDetail.ExpirationTime < DateTime.UtcNow.ToEST())
                    ViewBag.Error = "Your password Link has been Expired.";
            }

            return View();
            //return code == null ? View("Error") : View();
        }
        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            string errorMsg = CheckPasswordResetValidation(ApplicationSetting, model.Password);
            if (errorMsg != "")
                ModelState.AddModelError("Password", errorMsg);

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //var user = await UserManager.FindByNameAsync(model.Email);
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var hashOldPassword = user.PasswordHash;
            // password reset : start
            var userPassDetail = _userPassResetService.GetUserDetails(user.Id);
            if (userPassDetail == null)
            {
                ViewBag.Error = "User not exist.";
                return View();
            }
            var passwordCode = userPassDetail.Code;
            if (passwordCode != model.Code)
            {
                ViewBag.Error = "Invalid code. Please send a valid code to reset password.";
                return View();
            }

            if (userPassDetail.ExpirationTime < DateTime.UtcNow.ToEST())
            {
                ViewBag.Error = "Your password Link has been Expired.";
                return View();
            }

            // password reset : End

            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                ///Unlocking Account Start  
                ///The order of the execution of these three lines is must. Setting lockout end time before disabling lockout.
                await UserManager.ResetAccessFailedCountAsync(user.Id);
                await UserManager.SetLockoutEndDateAsync(user.Id, DateTime.UtcNow.ToEST());
                await UserManager.SetLockoutEnabledAsync(user.Id, false);
                ///Unlocking Account End 


                //Saving user log : Start
                var objUserLog = new AspNetUsers_Log
                {
                    PasswordHash_Old = hashOldPassword,
                    SecurityStamp_Old = user.SecurityStamp,
                    PasswordHash_New = user.PasswordHash,
                    CreatedBy = user.Id,
                    CreatedDate = DateTime.Now.ToEST(),
                    CreatedByName = user.FullName,
                    ModifiedBy = user.Id,
                    ModifiedDate = DateTime.Now.ToEST(),
                    ModifiedByName = user.FullName,
                    AspNetUsersId = user.Id
                };
                _userLogService.Create(objUserLog);
                //Saving user log : End

                //Start -> user Updation : setting RequirePasswordReset to true
                if (user.RequirePasswordReset == false)
                {
                    user.RequirePasswordReset = true;
                    user.ModifiedBy = user.Id;
                    user.ModifiedByName = user.FullName;
                    user.ModifiedDate = DateTime.Now.ToEST();
                    var resultUser = await UserManager.UpdateAsync(user);
                    //End -> user updated.
                    _userPassResetService.DeletePasswordResetUser(user.Id);
                }
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }
        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }
        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }
        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }
        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    LogAuditRecord(loginInfo.Email, AuditRecordLogStatus.ExternalLoginSucess.ToDescription());
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    LogAuditRecord(loginInfo.Email, AuditRecordLogStatus.ExternalAccountLockOut.ToDescription()); 
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    LogAuditRecord(loginInfo.Email, AuditRecordLogStatus.ExternalRequireVarification.ToDescription());
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    LogAuditRecord(loginInfo.Email, AuditRecordLogStatus.ExternalLoginFailed.ToDescription());
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }
        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    LogAuditRecord(info.Email, AuditRecordLogStatus.ExternalAccountCreated.ToDescription());

                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        LogAuditRecord(info.Email, AuditRecordLogStatus.LoginSucess.ToDescription());

                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }
        [OutputCache(NoStore = true, Duration = 1)]
        public ActionResult Signout(string isLogout = "")
        {
            var user = loggedInUser;

            string PCName = GetUniqueMachineInfo(user.Id.ToString());
            _userVerificationService.userSignOut(user.Id.ToString(), PCName, isLogout);

            // get delete token from specific Machine
            var tokens = _tokenservice.deleteToken(user.Id.ToString(), PCName);
            _tokenservice.DeleteRange(tokens);

            LogAuditRecord(user.UserName, AuditRecordLogStatus.LogOut.ToString());

            return View();
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult AccessDenied()
        {
            var username = "Unknown";
            if(loggedInUser != null)
            {
                username = loggedInUser.UserName;
            }

            LogAuditRecord(username, AuditRecordLogStatus.AccessDenied.ToDescription());
            return GetViewResult();
        }

        public async Task<ActionResult> PasswordConfirmation(string Username, string Password)
        {
            bool RememberMe = false;
            var result = await SignInManager.PasswordSignInAsync(Username, Password, RememberMe, shouldLockout: true);
            string message = "";
            switch (result)
            {
                case SignInStatus.Success:
                    message = "Success";
                    return Json(message, JsonRequestBehavior.AllowGet);
                case SignInStatus.Failure:
                    message = "Failure";
                    return Json(message, JsonRequestBehavior.AllowGet);
            }
            return Json(message, JsonRequestBehavior.AllowGet);
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
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }
            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }
            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        #region ----- Disposable -----
        private bool disposed = false; // to detect redundant calls
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    _userLogService?.Dispose();
                    _userPassResetService?.Dispose();
                    _alarmService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        [AllowAnonymous]
        public ActionResult ChangePasswordOnFirstLogin()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> ChangePasswordOnFirstLogin(ChangePassowrdFirstLoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userName = model.UserName;
                    if (string.IsNullOrEmpty(userName))
                    {
                        ModelState.AddModelError("", "Error! UserName not found. Try to login again.");
                        return GetErrorResult(model);
                    }
                    var dbuser = await UserManager.FindAsync(userName, model.OldPassword);
                    if (dbuser == null)
                        ModelState.AddModelError("Password", "Invalid Password");
                    var hashNewPassword = UserManager.PasswordHasher.HashPassword(model.NewPassword);

                    string errorMsg = CheckPasswordResetValidation(ApplicationSetting, model.NewPassword);
                    if (errorMsg != "")
                        ModelState.AddModelError("Password", errorMsg);

                    if (ModelState.IsValid)
                    {
                        // This is for password history
                        if (ApplicationSetting.aps_security_password_history_value.HasValue)
                        {
                            var userPasswordLog = _userLogService.GetUserLog(dbuser.Id, (int)ApplicationSetting.aps_security_password_history_value);
                            foreach (var item in userPasswordLog)
                            {
                                var passResult = UserManager.PasswordHasher.VerifyHashedPassword(item.PasswordHash_New, model.NewPassword);
                                switch (passResult)
                                {
                                    case PasswordVerificationResult.Success:
                                        ViewBag.IsPasswordExpired = false;
                                        ViewBag.userName = model.UserName;
                                        ModelState.AddModelError("Password", "You cannot use your previous " + ApplicationSetting.aps_security_password_history_value + " passwords.");
                                        return GetErrorResult(model);
                                }
                            }
                        }

                        // This is for password Age : start
                        if (ApplicationSetting.aps_security_password_age_value > 0)
                            dbuser.PasswordExpirationDate = DateTime.UtcNow.ToEST().AddDays(ApplicationSetting.aps_security_password_age_value);
                        else
                            dbuser.PasswordExpirationDate = null;
                        // This is for password Age : end

                        dbuser.ModifiedBy = dbuser.Id;
                        dbuser.ModifiedByName = dbuser.FullName;
                        dbuser.ModifiedDate = DateTime.Now.ToEST();
                        //Set the value to true which mean that password has been reset on first login
                        dbuser.RequirePasswordReset = true;
                        var hashOldPassword = dbuser.PasswordHash;

                        var objUserLog = new AspNetUsers_Log
                        {
                            PasswordHash_Old = hashOldPassword,
                            SecurityStamp_Old = dbuser.SecurityStamp,
                            PasswordHash_New = hashNewPassword,
                            CreatedBy = dbuser.Id,
                            CreatedDate = DateTime.Now.ToEST(),
                            CreatedByName = dbuser.FullName,
                            ModifiedBy = dbuser.Id,
                            ModifiedDate = DateTime.Now.ToEST(),
                            ModifiedByName = dbuser.FullName,
                            AspNetUsersId = dbuser.Id

                        };
                        _userLogService.Create(objUserLog);

                        dbuser.PasswordHash = hashNewPassword;
                        var resultUser = await UserManager.UpdateAsync(dbuser);
                        if (!resultUser.Succeeded)
                        {
                            ViewBag.IsPasswordExpired = false;
                            ViewBag.userName = model.UserName;
                            ModelState.AddModelError("", resultUser.Errors.First());
                            return View(model);
                        }

                        //TempData["passwordHasbeenChanged"] = "Your password has successfully been changed.";
                        return RedirectToAction("Login", new { isPasswordReset = true });
                    }
                    else
                    {
                        ViewBag.IsPasswordExpired = false;
                        ViewBag.userName = model.UserName;
                        return GetErrorResult(model);
                    }

                }
                ViewBag.IsPasswordExpired = false;
                ViewBag.userName = model.UserName;
                return GetErrorResult(model);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
            }
            return RedirectToAction("Login");
        }
        #endregion


        #region TCARE-511 Sending Reset Password Link

        private async void SendPasswordResetLink(ApplicationUser user)
        {
            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            var actionLink = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code });
            //var callbackUrl =  ConfigurationManager.AppSettings["ServerUrl"] +"/Account/ResetPassword?userId=" + Url.Encode(user.Id) + "&code=" + Url.Encode(code);

            var callbackUrl = ConfigurationManager.AppSettings["ServerUrl"] + actionLink;

            string subject = "TeleSpecialists – Password Reset Request";
            string emailText = "THIS E-MAIL CONTAINS IMPORTANT INFORMATION PERTAINING TO YOUR ABILITY TO ACCESS THE TELESPECIALISTS SYSTEM - ";
            emailText += "DO NOT SHARE THIS MESSAGE.<br /><br /><br /><br />You can reset the password for your account associated with username";
            emailText += " [" + user.UserName + "] <a href=\"" + callbackUrl + "\">here.</a> This link is only valid for next one hour. ";

            EmailHelper.SendEmail(user.Email, subject, emailText);

            // Adding data in Password reset -- Start
            var _userPasswordReset = new AspNetUsers_PasswordReset
            {
                Code = code,
                ExpirationTime = DateTime.UtcNow.ToEST().AddHours(1),
                UserName = user.UserName,
                AspNetUserId = user.Id,
                CreatedBy = user.Id,
                CreatedDate = DateTime.Now.ToEST(),
                CreatedByName = user.FullName,
                ModifiedBy = user.Id,
                ModifiedDate = DateTime.Now.ToEST(),
                ModifiedByName = user.FullName
            };
            _userPassResetService.Create(_userPasswordReset);
            // Adding data in Password reset -- End
        }
        #endregion
    }
}
