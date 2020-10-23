using Kendo.DynamicLinq;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.Models;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.Web.Models;
using TeleSpecialists.Web.Hubs;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;

namespace TeleSpecialists.Controllers
{
    [Authorize]
    [AccessRoles(Roles = "Super Admin,Finance")]
    public class AdminController : BaseController
    {
        private readonly AdminService _adminService;
        private readonly PhysicianService _physicianService;
        private readonly PhysicianStatusService _physicianStatus;
        private readonly PhysicianStatusLogService _physicianStatusLogService;
        private readonly AspNetUsersLogService _userLogService;
        private readonly AlarmTuneService _alarmTuneService;
        private readonly UserVerificationService _userVerificationService;

        public AdminController()
        {
            _adminService = new AdminService();
            _physicianService = new PhysicianService();
            _physicianStatus = new PhysicianStatusService();
            _physicianStatusLogService = new PhysicianStatusLogService();
            _userLogService = new AspNetUsersLogService();
            _alarmTuneService = new AlarmTuneService();
            _userVerificationService = new UserVerificationService();
        }

        public ActionResult Index()
        {
            return GetViewResult();
        }

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

        public async Task<ActionResult> RemoteUserLogin(string Id)
        {
            try
            {
                var currentUserId = User.Identity.GetUserId();
                var userToSighnIn = UserManager.FindById(Id);
                var currentUserName = User.Identity.Name;
                if (userToSighnIn != null)
                {
                    LogAuditRecord(currentUserName, AuditRecordLogStatus.RemoteLogOut.ToDescription());
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

                    var identity = await UserManager.CreateIdentityAsync(userToSighnIn, DefaultAuthenticationTypes.ApplicationCookie);
                    identity.AddClaim(new Claim("FullName", userToSighnIn.FullName));
                    AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = false }, identity);

                    var action = string.Format("{0} as {1}", AuditRecordLogStatus.RemoteLogin.ToDescription(), userToSighnIn.UserName);
                    LogAuditRecord(currentUserName, action);

                    #region refreshing all windows after remote login 
                    if (BLL.Settings.RPCMode == RPCMode.SignalR)
                    {
                        var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<PhysicianCasePopupHub>();
                        var connections = PhysicianCasePopupHub.ConnectedUsers.Where(m => m.UserId == currentUserId).ToList();
                        string browser = Request.UserAgent.ToLower();

                        connections.ForEach(m =>
                        {
                            hubContext.Clients.Client(m.ConnectionId).reloadPageForUser(browser);
                        });
                    }
                    else if (BLL.Settings.RPCMode == RPCMode.WebSocket)
                    {
                        var paramData = new List<object>();
                        paramData.Add(Request.UserAgent.ToLower());
                        new WebSocketEventHandler().CallJSMethod(currentUserId, new SocketResponseModel { MethodName = "reloadPageForUser_def", Data = paramData });
                    }

                    #endregion

                    #region Signout entry before remote login
                    string PCName = GetUniqueMachineInfo(currentUserId.ToString());
                    _userVerificationService.userSignOut(currentUserId.ToString(), PCName, "");
                    #endregion

                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return null;
        }

        [HttpGet]
        public ActionResult OnlineUsers()
        {
            var allUsers = WebSocketEventHandler.ConnectedClients.Select(x => ((WebSocketEventHandler)x));
            var users = allUsers.Where(m => string.IsNullOrEmpty(m.ServerUrl));
            var otherUsers = allUsers.Where(m => !string.IsNullOrEmpty(m.ServerUrl));
                          


            return Json(new { users = users.Select(m=> new { m.ServerUrl, m.UserId }), otherUsers = otherUsers.Select(m => new { m.ServerUrl, m.UserId }) }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EncodeString(string inputString)
        {
            string returnValue = Functions.EncodeTo64UTF8(inputString);
            return Json(new { data = returnValue });
        }

        #region Users
        public ActionResult Users()
        {
            ViewBag.Role = RoleManager.Roles
                .Select(m => new SelectListItem
                {
                    Text = m.Name,
                    Value = m.Id
                }).ToList();

            ViewBag.Error = (TempData["Error"] as bool?) ?? false;
            ViewBag.Message = TempData["StatusMessage"] as string;
            return GetViewResult();
        }

        /// <summary>
        ///  Get Settting for alams
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Settings()
        {
            var alarm_tune_list = _alarmTuneService.GetTuneList();
            ViewBag.NotificationTunes = alarm_tune_list;
            return GetViewResult(ApplicationSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Settings(application_setting model, string audio_name)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (var _appSettingService = new AppSettingService())
                    {
                        if (model != null)
                        {
                            var dbModel = _appSettingService.Get();

                            dbModel.aps_enable_auto_assign_process = model.aps_enable_auto_assign_process;
                            dbModel.aps_md_base_url = model.aps_md_base_url;
                            dbModel.aps_md_facility = model.aps_md_facility;
                            dbModel.aps_md_grant_type = model.aps_md_grant_type;
                            dbModel.aps_md_instance = model.aps_md_instance;
                            dbModel.aps_md_login = model.aps_md_login;
                            dbModel.aps_md_password = model.aps_md_password;
                            dbModel.aps_md_token_url = model.aps_md_token_url;
                            dbModel.aps_is_md_staff_active = model.aps_is_md_staff_active;
                            dbModel.aps_cas_facility_popup_on_load = model.aps_cas_facility_popup_on_load;

                            //Five9 fields
                            dbModel.aps_five9_domain = model.aps_five9_domain;
                            dbModel.aps_five9_list = model.aps_five9_list;
                            dbModel.aps_five9_number_to_dial = model.aps_five9_number_to_dial;

                            dbModel.aps_modified_by = User.Identity.GetUserId();
                            dbModel.aps_modified_date = DateTime.Now.ToEST();

                            // Adding security fields - Start
                            dbModel.aps_security_is_lowercase_required = model.aps_security_is_lowercase_required;
                            dbModel.aps_security_is_uppercase_required = model.aps_security_is_uppercase_required;
                            dbModel.aps_security_is_number_required = model.aps_security_is_number_required;
                            dbModel.aps_security_is_non_alphanumeric_required = model.aps_security_is_non_alphanumeric_required;
                            dbModel.aps_security_password_length_value = model.aps_security_password_length_value;
                            dbModel.aps_security_password_age_value = model.aps_security_password_age_value;
                            dbModel.aps_security_password_history_value = model.aps_security_password_history_value;
                            dbModel.aps_secuirty_is_multi_factor_authentication_required = model.aps_secuirty_is_multi_factor_authentication_required;
                            dbModel.aps_secuirty_is_reset_password_required = model.aps_secuirty_is_reset_password_required;
                            dbModel.aps_two_factor_code_expiry_timeout_in_minutes = model.aps_two_factor_code_expiry_timeout_in_minutes;
                            dbModel.aps_session_timeout_in_minutes = model.aps_session_timeout_in_minutes;

                            //Other tab
                            dbModel.aps_enable_case_auto_save = model.aps_enable_case_auto_save;
                            dbModel.aps_duplicate_popup_timer = model.aps_duplicate_popup_timer;

                            dbModel.aps_statusgrid_filter_start_time = model.aps_statusgrid_filter_start_time;
                            dbModel.aps_statusgrid_filter_endtime = model.aps_statusgrid_filter_endtime;
                            dbModel.aps_enable_alarm_setting = model.aps_enable_alarm_setting; //  alarm settings

                            dbModel.aps_rapids_email = model.aps_rapids_email;
                            dbModel.aps_rapids_password = model.aps_rapids_password == "**********" ? dbModel.aps_rapids_password : model.aps_rapids_password;
                            dbModel.aps_rapids_service = model.aps_rapids_service;
                            dbModel.aps_rapids_retention = model.aps_rapids_retention;

                            if (model.aps_security_password_age_value > 0)
                            {
                                var expiraionDate = DateTime.UtcNow.ToEST().AddDays(model.aps_security_password_age_value);
                                SqlParameter paramExDate = new SqlParameter("@userPasswordExpirationDate", expiraionDate);
                                DBHelper.ExecuteNonQuery("usp_update_users_password_expiration", paramExDate);
                            }
                            else
                                DBHelper.ExecuteNonQuery("usp_update_users_password_expiration", null);

                            // add code for default tune notification
                            if (!string.IsNullOrEmpty(audio_name))
                            {
                                string[] file_name = audio_name.Split(',');
                                dbModel.aps_audio_file_path = file_name[0];
                                dbModel.aps_selected_audio = file_name[1];
                                dbModel.aps_tune_is_active = true;
                            }
                            // Adding security fields - End
                            _appSettingService.Edit(dbModel);
                        }
                        else
                        {
                            model.aps_key = Guid.NewGuid();
                            _appSettingService.Create(model);
                        }

                        this.RefreshApplicationSetting();
                    }
                    return ShowSuccessMessageOnly("Settings successfully updated", model);
                }

                return GetErrorResult(model);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return GetErrorResult(null);
            }

        }

        [HttpPost]
        public JsonResult GetRapidsPassword()
        {
            string aps_rapids_password = ApplicationSetting.aps_rapids_password;
            return Json(new { success = true, data = aps_rapids_password }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetUsers(DataSourceRequest request, List<string> roleIDs)
        {
            var res = _adminService.GetAllUsers(request, roleIDs);
            return Json(res, JsonRequestBehavior.AllowGet);
        }


        #region Shiraz Code
       
        public ActionResult AddUser()
        {
            try
            {
                RegisterViewModel model = new RegisterViewModel();
                model.isActive = true;
                ViewBag.Role = RoleManager.Roles
                                           .Select(m => new SelectListItem
                                           {
                                               Text = m.Name,
                                               Value = m.Id
                                           }).ToList();


                model.APISecretKey = Functions.GetRandomKey();

                return GetViewResult(model);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return GetErrorResult(null);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddUser(RegisterViewModel userViewModel)
        {
            bool PasswordError = false;
            try
            {
                if (ModelState.IsValid)
                {
                    if (userViewModel.Role == "0029737b-f013-4e0b-8a31-1b09524194f9")
                    {
                        var intialsCount = _adminService.GetUsersByInitial(userViewModel.UserInitial).Count();
                        if (intialsCount > 0)
                            ModelState.AddModelError("UserInitial", $"There is already a User in the system with the same initials. Please enter new Initials using up to three (3) alphanumeric characters.");
                        //Suggested Initial is {userViewModel.UserInitial}{intialsCount + 1}");
                    }

                    if (_adminService.ValidateNPINumber(userViewModel.UserInitial))
                        ModelState.AddModelError("NPINumber", "There is already a User in the system with the same NPI Number. Please enter a different NPI Number ");

                    if (ModelState.IsValid)
                    {

                        var user = new ApplicationUser
                        {
                            FirstName = userViewModel.FirstName,
                            LastName = userViewModel.LastName,
                            UserName = userViewModel.Username,
                            Email = userViewModel.Email,
                            IsActive = userViewModel.isActive,
                            IsDisable = userViewModel.isDisable,
                            EnableFive9 = userViewModel.EnableFive9,
                            MobilePhone = userViewModel.MobilePhone,
                            PhoneNumber = userViewModel.OtherPhone,
                            UserInitial = userViewModel.UserInitial,
                            NPINumber = userViewModel.NPINumber,
                            CaseReviewer = userViewModel.CaseReviewer,
                            CreatedBy = User.Identity.GetUserId(),
                            CreatedDate = DateTime.Now.ToEST(),
                            CreatedByName = loggedInUser.FullName,
                            IsEEG = userViewModel.IsEEG,
                            IsStrokeAlert = userViewModel.IsStrokeAlert,
                            NHAlert = userViewModel.NHAlert,
                            Address_line1 = userViewModel.Address_line1,
                            Address_line2 = userViewModel.Address_line2,
                            City = userViewModel.City,
                            Zip = userViewModel.Zip,
                            State_key = userViewModel.State_key,
                            IsSleep = userViewModel.IsSleep,
                            TwoFactorEnabled = userViewModel.TwoFactorEnabled
                        };

                        if (!string.IsNullOrEmpty(userViewModel.Role))
                        {
                            var userRole = RoleManager.Roles.FirstOrDefault(m => m.Id == userViewModel.Role);
                            if (userRole.Name == UserRoles.TeleCareApi.ToDescription())
                            {
                                user.APISecretKey = userViewModel.APISecretKey;
                                user.APIPassword = Functions.EncodeTo64UTF8($"{userViewModel.Username}:{userViewModel.Password}:{userViewModel.APISecretKey}");
                            }
                        }

                        bool errorMsg = CheckPasswordValidation(ApplicationSetting, userViewModel.Password);
                        if (errorMsg == true)
                        {
                            ModelState.AddModelError("a", "Password must be " + ApplicationSetting.aps_security_password_length_value + " to 16 characters and contain:");
                            ModelState.AddModelError("b", "1 Capital Letter");
                            ModelState.AddModelError("c", "1 Lower Case Letter");
                            ModelState.AddModelError("d", "1 Number");
                            ModelState.AddModelError("e", "1 Special Character(!,@,#,$,%,^,&,*,_,+,=)");
                            PasswordError = true;
                        }

                        if (!PasswordError)
                        {
                            var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);

                            if (adminresult.Succeeded)
                            {
                                // Assigning Role to User
                                await AddUserToRole(new MyUserRole { RoleId = userViewModel.Role, UserId = user.Id });

                                var action = string.Format("{0} as {1}", AuditRecordLogStatus.UserCreated.ToDescription(), user.UserName);
                                LogAuditRecord(loggedInUser.UserName, action);
                                TempData["StatusMessage"] = "User has been Created";
                                //return RedirectToAction("Users");
                                string newId = user.Id;
                                var newCreatedUsre = await UserManager.FindByIdAsync(newId);

                                var objUserLog = new AspNetUsers_Log
                                {
                                    PasswordHash_Old = newCreatedUsre.PasswordHash,
                                    SecurityStamp_Old = newCreatedUsre.SecurityStamp,
                                    PasswordHash_New = newCreatedUsre.PasswordHash,
                                    CreatedBy = newCreatedUsre.Id,
                                    CreatedByName = newCreatedUsre.FullName,
                                    CreatedDate = DateTime.Now.ToEST(),
                                    ModifiedBy = newCreatedUsre.Id,
                                    ModifiedByName = newCreatedUsre.FullName,
                                    ModifiedDate = DateTime.Now.ToEST(),
                                    AspNetUsersId = newCreatedUsre.Id
                                };

                                _userLogService.Create(objUserLog);

                                return GetSuccessResult(Url.Action("Users", new { level = userViewModel.Level }));
                            }
                            else
                            {
                                ModelState.AddModelError("", adminresult.Errors.First());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
            }

            ViewBag.Role = RoleManager.Roles
                                           .Select(m => new SelectListItem
                                           {
                                               Text = m.Name,
                                               Value = m.Id
                                           }).ToList();


            if (PasswordError)
                return GetErrorResult(userViewModel, true);
            else
                return GetErrorResult(userViewModel);
        }
        [HttpGet]
        public ActionResult _UserCredentialIndex(string Id)
        {
            var model = UserManager.Users.Where(m => m.Id == Id)
                                         .Select(m => new CredentialIndexViewModel
                                         {
                                             CredentialIndex = m.CredentialIndex,
                                             UserId = m.Id
                                         }).FirstOrDefault();
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult _UserCredentialIndex(CredentialIndexViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = UserManager.FindById(model.UserId);
                    user.CredentialIndex = model.CredentialIndex;
                    user.ModifiedDate = DateTime.Now.ToEST();
                    user.ModifiedByName = loggedInUser.FullName;
                    UserManager.Update(user);
                    return GetSuccessResult(Url.Action("_UserCredentialIndex", new { Id = model.UserId }), "CredentialIndex successfully updated");
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
            }

            return GetErrorResult(model);
        }

        [HttpGet]
        public ActionResult EditUser(string id)
        {
            try
            {
                EditUserViewModel model = new EditUserViewModel();
                var userDetails = UserManager.Users.Where(x => x.Id == id).Select(x => new EditUserViewModel
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Username = x.UserName,
                    Email = x.Email,
                    isActive = x.IsActive,
                    isDisable = x.IsDisable,
                    Role = x.Roles.FirstOrDefault() != null ? x.Roles.FirstOrDefault().RoleId : "",
                    EnableFive9 = x.EnableFive9,
                    MobilePhone = x.MobilePhone,
                    OtherPhone = x.PhoneNumber,
                    UserInitial = x.UserInitial,
                    NPINumber = x.NPINumber,
                    Address_line1 = x.Address_line1,
                    Address_line2 = x.Address_line2,
                    City = x.City,
                    Zip = x.Zip,
                    State_key = x.State_key,
                    CaseReviewer = x.CaseReviewer,
                    IsEEG = x.IsEEG != null ? (bool)x.IsEEG : false,
                    IsStrokeAlert = x.IsStrokeAlert != null ? (bool)x.IsStrokeAlert : false,
                    NHAlert = x.NHAlert != null ? (bool)x.NHAlert : false,
                    IsSleep = x.IsSleep != null ? (bool)x.IsSleep : false,
                    LockoutEnabled = x.LockoutEnabled,
                    TwoFactorEnabled = x.TwoFactorEnabled
                }).FirstOrDefault();

                if (userDetails != null)
                {
                    ViewBag.Role = RoleManager.Roles
                                         .Select(m => new SelectListItem
                                         {
                                             Text = m.Name,
                                             Value = m.Id
                                         }).ToList();

                    model = userDetails;

                    var userRole = RoleManager.Roles.FirstOrDefault(m => m.Id == userDetails.Role);
                    if (userRole != null)
                    {
                        model.Level = userRole.Name;
                    }

                    return GetViewResult(model);
                }
                else
                {
                    TempData["Error"] = true;
                    TempData["StatusMessage"] = "User not found.";
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return GetViewResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUser(EditUserViewModel userViewModel)
        {
            try
            {

                ViewBag.Roles = RoleManager.Roles
                                     .ToDictionary(m => m.Id, m => m.Name);

                if (ModelState.IsValid)
                {
                    var user = await UserManager.FindByIdAsync(userViewModel.Id);
                    if (user == null)
                    {
                        return HttpNotFound();
                    }
                    if (userViewModel.Role == "0029737b-f013-4e0b-8a31-1b09524194f9")
                    {
                        var intialsCount = _adminService.GetUsersByInitial(userViewModel.UserInitial, user.Id).Count();
                        if (intialsCount > 0)
                            ModelState.AddModelError("UserInitial", $"There is already a User in the system with the same initials. Please enter new Initials using up to three (3) alphanumeric characters.");
                        // Suggested Initial is {userViewModel.UserInitial}{intialsCount + 1}"
                    }

                    if (_adminService.ValidateNPINumber(userViewModel.NPINumber, user.Id))
                        ModelState.AddModelError("NPINumber", "There is already a User in the system with the same NPI Number. Please enter a different NPI Number ");

                    if (ModelState.IsValid)
                    {

                        var prevState = user.EnableFive9;
                        user.FirstName = userViewModel.FirstName;
                        user.LastName = userViewModel.LastName;
                        user.Email = userViewModel.Email;
                        user.IsActive = userViewModel.isActive;
                        user.IsDisable = userViewModel.isDisable;
                        user.EnableFive9 = userViewModel.EnableFive9;
                        user.PhoneNumber = userViewModel.OtherPhone;
                        user.MobilePhone = userViewModel.MobilePhone;
                        user.NPINumber = userViewModel.NPINumber;
                        user.UserInitial = userViewModel.UserInitial;
                        user.Address_line1 = userViewModel.Address_line1;
                        user.Address_line2 = userViewModel.Address_line2;
                        user.City = userViewModel.City;
                        user.Zip = userViewModel.Zip;
                        user.State_key = userViewModel.State_key;
                        user.CaseReviewer = userViewModel.CaseReviewer;
                        user.IsEEG = userViewModel.IsEEG;
                        user.IsStrokeAlert = userViewModel.IsStrokeAlert;
                        user.NHAlert = userViewModel.NHAlert;
                        user.ModifiedBy = User.Identity.GetUserId();
                        user.ModifiedDate = DateTime.Now.ToEST();
                        user.ModifiedByName = loggedInUser.FullName;
                        user.IsSleep =  userViewModel.IsSleep;
                        user.TwoFactorEnabled = userViewModel.TwoFactorEnabled;
                        if (!userViewModel.LockoutEnabled)
                        {
                            user.LockoutEnabled = userViewModel.LockoutEnabled;
                            user.AccessFailedCount = 0;
                            user.LockoutEndDateUtc = null;
                        }

                        if (!string.IsNullOrEmpty(userViewModel.Role) && user?.Roles?.FirstOrDefault()?.RoleId != userViewModel.Role)
                        {
                            var assignedRoles = user.Roles.Select(m => m.RoleId);
                            var allUserRoles = RoleManager.Roles
                                                          .Where(m => assignedRoles.Contains(m.Id))
                                                          .Select(m => m.Name)
                                                          .ToArray();


                            UserManager.RemoveFromRoles(user.Id, allUserRoles);

                            // assign new role to user. 
                            await AddUserToRole(new MyUserRole { RoleId = userViewModel.Role, UserId = user.Id });
                        }
                        else
                        {
                            await UserManager.UpdateAsync(user); // just update the user without updating the roles.

                            var action = string.Format("{0} for {1}", AuditRecordLogStatus.UserUpdated.ToDescription(), user.UserName);
                            LogAuditRecord(loggedInUser.UserName, action);
                        }

                        IdentityResult identityResult = await UserManager.UpdateAsync(user);
                        if (!identityResult.Succeeded)
                        {
                            ModelState.AddModelError("", identityResult.Errors.First());

                            return GetViewResult(userViewModel);
                        }
                        this.RefreshLoggedInUser(user.Id);

                        TempData["StatusMessage"] = "User has been Updated";

                        if (prevState != userViewModel.EnableFive9)
                        {

                            return Json(new AjaxResult { success = true, refershPage = true, redirectUrl = Url.Action("Users", new { level = userViewModel.Level }) });
                        }

                        else
                            return GetSuccessResult(Url.Action("Users", new { level = userViewModel.Level }));
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
            }


            return GetErrorResult(userViewModel);
        }
        #endregion 
        public async Task<ActionResult> ChangePassword(string id)
        {
            try
            {

                var user = await UserManager.FindByIdAsync(id);
                var IsApiUser = UserManager.GetRoles(id).Contains(UserRoles.TeleCareApi.ToDescription());

                return GetViewResult(new UpdateUserViewModel()
                {
                    Id = user.Id,
                    Username = user.UserName,
                    IsApiUser = IsApiUser,
                    APISecretKey = user.APISecretKey,
                    APIPassword = user.APIPassword
                });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return GetSuccessResult(Url.Action("Users"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(UpdateUserViewModel userviewmodel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dbuser = await UserManager.FindByIdAsync(userviewmodel.Id);
                    var hashOldPassword = dbuser.PasswordHash;
                    var hashNewPassword = UserManager.PasswordHasher.HashPassword(userviewmodel.Password);

                    var IsApiUser = UserManager.GetRoles(dbuser.Id).Contains(UserRoles.TeleCareApi.ToDescription());
                    if (IsApiUser)
                    {
                        if (string.IsNullOrEmpty(userviewmodel.APISecretKey))
                            ModelState.AddModelError("APISecretKey", "API Secret Key field is required");
                        if (string.IsNullOrEmpty(userviewmodel.APIPassword))
                            ModelState.AddModelError("APIPassword", "API Password field is required");

                        if (ModelState.IsValid)
                        {
                            dbuser.APISecretKey = userviewmodel.APISecretKey;
                            dbuser.APIPassword = Functions.EncodeTo64UTF8($"{userviewmodel.Username}:{userviewmodel.Password}:{userviewmodel.APISecretKey}");
                        }
                    }

                    bool errorMsg = CheckPasswordValidation(ApplicationSetting, userviewmodel.Password);
                    if (errorMsg == true)
                    {
                        ModelState.AddModelError("a", "Password must be " + ApplicationSetting.aps_security_password_length_value + " to 16 characters and contain:");
                        ModelState.AddModelError("b", "1 Capital Letter");
                        ModelState.AddModelError("c", "1 Lower Case Letter");
                        ModelState.AddModelError("d", "1 Number");
                        ModelState.AddModelError("e", "1 Special Character(!,@,#,$,%,^,&,*,_,+,=)");
                        return GetErrorResult(userviewmodel, true);
                    }
                    //bool err = CheckPasswordErrors(settings, userviewmodel.Password);
                    //if (err)
                    //    return GetErrorResult(userviewmodel, true);

                    if (ModelState.IsValid)
                    {
                        // This is for password Age
                        if (ApplicationSetting.aps_security_password_age_value > 0)
                        {
                            dbuser.PasswordExpirationDate = DateTime.UtcNow.ToEST().AddDays(ApplicationSetting.aps_security_password_age_value);
                        }
                        else
                            dbuser.PasswordExpirationDate = null;

                        // This is for password history
                        if (ApplicationSetting.aps_security_password_history_value.HasValue)
                        {
                            var userPasswordLog = _userLogService.GetUserLog(dbuser.Id, (int)ApplicationSetting.aps_security_password_history_value);
                            foreach (var item in userPasswordLog)
                            {
                                var passResult = UserManager.PasswordHasher.VerifyHashedPassword(item.PasswordHash_New, userviewmodel.Password);
                                switch (passResult)
                                {
                                    case PasswordVerificationResult.Success:
                                        ModelState.AddModelError("Password", "You cannot use your previous " + ApplicationSetting.aps_security_password_history_value + " passwords.");
                                        return GetErrorResult(userviewmodel);
                                }
                            }
                        }

                        //Saving user log : Start
                        var objUserLog = new AspNetUsers_Log
                        {
                            PasswordHash_Old = hashOldPassword,
                            SecurityStamp_Old = dbuser.SecurityStamp,
                            PasswordHash_New = hashNewPassword,
                            CreatedBy = User.Identity.GetUserId(),
                            CreatedDate = DateTime.Now.ToEST(),
                            CreatedByName = loggedInUser.FullName,
                            ModifiedBy = User.Identity.GetUserId(),
                            ModifiedDate = DateTime.Now.ToEST(),
                            ModifiedByName = loggedInUser.FullName,
                            AspNetUsersId = dbuser.Id
                        };
                        _userLogService.Create(objUserLog);
                        //Saving user log : End

                        dbuser.PasswordHash = hashNewPassword;
                        //After change the password the user must need to reset the password if it is set to true from security
                        dbuser.RequirePasswordReset = false;
                        dbuser.ModifiedBy = dbuser.Id;
                        dbuser.ModifiedByName = dbuser.FullName;
                        dbuser.ModifiedDate = DateTime.Now.ToEST();
                        if (string.IsNullOrEmpty(dbuser.SecurityStamp))
                            dbuser.SecurityStamp = Guid.NewGuid().ToString();

                        var result = await UserManager.UpdateAsync(dbuser);
                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError("", result.Errors.First());
                            return GetErrorResult(userviewmodel);
                        }

                        var action = string.Format("{0} for {1}", AuditRecordLogStatus.PasswordChange.ToDescription(), dbuser.UserName);
                        LogAuditRecord(loggedInUser.UserName, action);

                        return GetSuccessResult(Url.Action("Users"), "Password has been updated!");
                    }
                }

                return GetErrorResult(userviewmodel);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
            }
            return GetSuccessResult(Url.Action("Users"));
        }

        #endregion Users

        #region Roles
        public ActionResult Roles()
        {
            ViewBag.Error = (TempData["Error"] as bool?) ?? false;
            ViewBag.Message = TempData["StatusMessage"] as string;
            return GetViewResult();
        }

        [HttpPost]
        public ActionResult GetRoles(DataSourceRequest request)
        {
            var roles = RoleManager.Roles.Select(x => new
            {
                x.Id,
                x.Name,
                x.Description
            }).OrderBy(x => x.Name);

            var res = roles.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddRole()
        {
            try
            {
                return GetViewResult();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return GetErrorResult(null);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddRole(ApplicationRole applicationRole)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //var role = new IdentityRole(applicationRole.Name);
                    var roleresult = await RoleManager.CreateAsync(applicationRole);
                    if (!roleresult.Succeeded)
                    {
                        ModelState.AddModelError("", roleresult.Errors.First());
                        return GetViewResult(applicationRole);
                    }

                    TempData["StatusMessage"] = "Role has been Created!";
                    return GetSuccessResult(Url.Action("Roles"));
                }
                ModelState.AddModelError("", "Invalid data");
                return GetViewResult(applicationRole);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            ModelState.AddModelError("", "Error! Please try again.");
            return GetViewResult(applicationRole);
        }

        public async Task<ActionResult> EditRole(string id)
        {
            try
            {
                ApplicationRole applicationRole = new ApplicationRole();

                var role = await RoleManager.FindByIdAsync(id);
                if (role != null)
                {
                    applicationRole = new ApplicationRole()
                    {
                        Id = role.Id,
                        Name = role.Name,
                        Description = role.Description
                    };
                    return GetViewResult(applicationRole);
                }
                else
                {
                    TempData["Error"] = true;
                    TempData["StatusMessage"] = "Role not found.";
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
            }

            return GetSuccessResult(Url.Action("Roles"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditRole(ApplicationRole applicationRole)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string loggedinUser = System.Web.HttpContext.Current.User.Identity.GetUserId();

                    var role = await RoleManager.FindByIdAsync(applicationRole.Id);
                    role.Name = applicationRole.Name;
                    role.Description = applicationRole.Description;

                    await RoleManager.UpdateAsync(role);

                    TempData["StatusMessage"] = "Role has been Updated";
                    return GetSuccessResult(Url.Action("Roles"));
                }
                ModelState.AddModelError("", "Error! Please try again.");
                return GetViewResult(applicationRole);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            ModelState.AddModelError("", "Error! Please try again.");
            return GetSuccessResult(Url.Action("Roles"));
        }
        #endregion Roles

        #region ErrorConsole
        public ActionResult ErrorConsole()
        {
            return GetViewResult();
        }
        #endregion ErrorConsole

        #region ----- Role Settings -----
        [HttpPost]
        public async Task<JsonResult> AddUserRole(MyUserRole model)
        {
            try
            {

                await AddUserToRole(model);

                return Json(new { success = true, data = "", error = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(new { success = false, data = "", error = "Error!" }, JsonRequestBehavior.AllowGet);
        }
        private async Task AddUserToRole(MyUserRole model)
        {
            var user = await UserManager.FindByIdAsync(model.UserId);
            model.UserRoleId = Guid.NewGuid().ToString();


            //check if same role exists already
            var prevRoleExists = user.Roles.Where(x => x.UserId == model.UserId &&
                                                       x.RoleId == model.RoleId
                                                       ).Any();

            #region Adding entry in physician table in case of role is physician
            var role = await RoleManager.FindByNameAsync(UserRoles.Physician.ToDescription());
            var physician = _physicianService.GetDetail(model.UserId);
            // if the user role has been changed from physician to some different role
            if (role.Id != model.RoleId && physician != null)
            {
                user.status_key = null;
                user.status_change_date = DateTime.Now.ToEST();
                user.status_change_date_forAll = DateTime.Now.ToEST();
                user.status_change_cas_key = null;
                UserManager.Update(user);

            }
            else if (role.Id == model.RoleId && physician != null)
            {
                if (!user.status_key.HasValue)
                {
                    var defaultStatus = _physicianStatus.GetDefault();

                    user.status_key = defaultStatus.phs_move_status_key;
                    user.status_change_date = DateTime.Now.ToEST();
                    user.status_change_date_forAll = DateTime.Now.ToEST();
                    user.status_change_cas_key = null;
                    UserManager.Update(user);
                }
            }

            #endregion

            //if not only then add that role
            if (!prevRoleExists)
            {
                user.Roles.Add(model);
                IdentityResult identityResult = await UserManager.UpdateAsync(user);
            }
        }
        [HttpPost]
        public async Task<JsonResult> RemoveUserRole(string userRoleId, string userId)
        {
            try
            {
                var user = await UserManager.FindByIdAsync(userId);

                if (user.Roles.Count > 0)
                {
                    user.Roles.Where(x => x.UserRoleId == userRoleId).ToList();

                    IdentityResult identityResult = await UserManager.UpdateAsync(user);
                }

                return Json(new { success = true, data = "", error = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(new { success = false, data = "", error = "Error!" }, JsonRequestBehavior.AllowGet);
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
                    _adminService?.Dispose();
                    _physicianService?.Dispose();
                    _physicianStatus?.Dispose();
                    _physicianStatusLogService?.Dispose();
                    _userLogService?.Dispose();
                    _alarmTuneService?.Dispose();
                }
                disposed = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
