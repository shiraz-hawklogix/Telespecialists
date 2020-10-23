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
using TeleSpecialists.Controllers;
using System.Data.Entity.Validation;
using System.Web;
using System.IO;
using System.Configuration;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using System.Web.Configuration;

namespace TeleSpecialists.Web.Controllers
{
    public class UserProfileController : BaseController
    {

        private readonly AdminService _adminService;
        private readonly PhysicianService _physicianService;
        private readonly PhysicianStatusService _physicianStatus;
        private readonly PhysicianStatusLogService _physicianStatusLogService;
        private readonly AspNetUsersLogService _userLogService;
        private readonly PhysicianLicenseService _physicianLicenseService;
        private readonly FacilityPhysicianService _facilityPhysicianService;
        private readonly UserProfileServices _UserProfileService;
        public UserProfileController()
        {
            _adminService = new AdminService();
            _physicianService = new PhysicianService();
            _physicianStatus = new PhysicianStatusService();
            _physicianStatusLogService = new PhysicianStatusLogService();
            _userLogService = new AspNetUsersLogService();
            _physicianLicenseService = new PhysicianLicenseService();
            _facilityPhysicianService = new FacilityPhysicianService();
            _UserProfileService = new UserProfileServices();
        }
        [HttpPost]
        public ActionResult GetAll(DataSourceRequest request)
        {
            var res = _physicianLicenseService.GetAll(request);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetAllPhysicianFacilities(DataSourceRequest request)
        {
            var res = _facilityPhysicianService.GetPhysicianFacilitiesForPhy(request);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult EditUserProfile()
        {
            try
            {
                var id = loggedInUser?.Id;
                EditProfileViewModel model = new EditProfileViewModel();
                var userDetails = UserManager.Users.Where(x => x.Id == id).Select(x => new EditProfileViewModel
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Username = x.UserName,
                    Email = x.Email,
                    isActive = x.IsActive,
                    isDisable = x.IsDisable,
                    Gender=x.Gender,
                    CreatedDate = x.CreatedDate,
                    Role = x.Roles.FirstOrDefault() != null ? x.Roles.FirstOrDefault().RoleId : "",
                    EnableFive9 = x.EnableFive9,
                    CreatedBy = x.CreatedBy,
                    CreatedByName = x.CreatedByName,
                    MobilePhone = x.MobilePhone,
                    OtherPhone = x.PhoneNumber,
                    UserInitial = x.UserInitial,
                    NPINumber = x.NPINumber,
                    CaseReviewer = x.CaseReviewer,
                    IsEEG = x.IsEEG != null ? (bool)x.IsEEG : false,
                    IsStrokeAlert = x.IsStrokeAlert != null ? (bool)x.IsStrokeAlert : false,
                    NHAlert = x.NHAlert != null ? (bool)x.NHAlert : false,
                    Address_line1 = x.Address_line1,
                    Address_line2 = x.Address_line2,
                    City = x.City,
                    State_key = x.State_key,
                    Zip = x.Zip,
                    TwoFactorEnabled=x.TwoFactorEnabled,
                    PasswordHash = x.PasswordHash,
                    SecurityStamp = x.SecurityStamp,
                    User_Image = x.User_Image,
                    CredentialIndex=x.CredentialIndex,
                    LockoutEndDateUtc = x.LockoutEndDateUtc,
                    LockoutEnabled=x.LockoutEnabled,
                    AccessFailedCount=x.AccessFailedCount,
                    status_key=x.status_key,
                    status_change_cas_key=x.status_change_cas_key,
                    ModifiedByName = x.ModifiedByName,
                    ModifiedBy = x.ModifiedBy,
                    ModifiedDate = x.ModifiedDate,
                    RequirePasswordReset = x.RequirePasswordReset,
                    PasswordExpirationDate = x.PasswordExpirationDate,
                    status_change_date_forAll = x.status_change_date_forAll,
                    IsDeleted=x.IsDeleted,
                    ContractDate=x.ContractDate,
                    APIPassword = x.APIPassword,
                    CredentialCount=x.CredentialCount,
                    status_change_date=x.status_change_date,
                    AddressBlock = x.AddressBlock,
                    PhoneNumberConfirmed=x.PhoneNumberConfirmed,
                    EmailConfirmed=x.EmailConfirmed,
                    IsSleep= x.IsSleep,
                    IsTwoFactVerified= x.IsTwoFactVerified,
                    TwoFactVerifyCode= x.TwoFactVerifyCode,
                    CodeExpiryTime=x.CodeExpiryTime

                }).FirstOrDefault();

                if (userDetails != null)
                {
                    ViewBag.Role = RoleManager.Roles
                                         .Select(m => new SelectListItem
                                         {
                                             Text = m.Name,
                                         }).ToList();

                    model = userDetails;

                    var userRole = RoleManager.Roles.FirstOrDefault(m => m.Id == userDetails.Role);
                    if (userRole != null)
                    {
                        model.Role = userRole.Name;
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
        public async Task<ActionResult> EditUserProfile(EditProfileViewModel userViewModel, HttpPostedFileBase fileUpload)
        {
            try
            {

                var fileNames = "";
                if (fileUpload != null)
                {
                    fileNames = DateTime.Now.ToString("yyyymmddMMssfff") + System.IO.Path.GetExtension(fileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/"), fileNames);
                    var imagepth = path;
                    fileUpload.SaveAs(imagepth);

                    string connString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
                    var connStringArray = connString.Split(';');
                    var dictionary = new Dictionary<string, string>();
                    foreach (var item in connStringArray)
                    {
                        var itemKeyValue = item.Split('=');
                        dictionary.Add(itemKeyValue[0], itemKeyValue[1]);
                    }
                    string accountname = dictionary["AccountName"];
                    string accesskeys = dictionary["AccountKey"];
                    string accesskey = accesskeys + "==";
                    string BlobKey = WebConfigurationManager.AppSettings["BlobStroageKey"];
                    var OrigionalImagepath = BlobKey + fileNames;
                    StorageCredentials creden = new StorageCredentials(accountname, accesskey);
                    CloudStorageAccount acc = new CloudStorageAccount(creden, useHttps: true);
                    CloudBlobClient client = acc.CreateCloudBlobClient();
                    CloudBlobContainer cont = client.GetContainerReference("onboarding");
                    cont.CreateIfNotExists();
                    cont.SetPermissions(new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob

                    });
                    CloudBlockBlob cblob = cont.GetBlockBlobReference(fileNames);
                    var LocalImagePath = path.ToString();
                    using (Stream ddd = System.IO.File.OpenRead(@LocalImagePath))

                    {
                        cblob.UploadFromStream(ddd);
                    }
                    fileNames = OrigionalImagepath;
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
                else
                {
                    var id = loggedInUser?.Id;
                    fileNames = UserManager.Users.Where(x => x.Id == id).Select(c => c.User_Image).FirstOrDefault();
                }
                AspNetUser editProfileViewModel = new AspNetUser();
                ViewBag.Roles = RoleManager.Roles.ToDictionary(m => m.Id, m => m.Name);
                var user = await UserManager.FindByIdAsync(userViewModel.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                var intialsCount = _adminService.GetUsersByInitial(userViewModel.UserInitial, user.Id).Count();
                if (intialsCount > 0)
                    ModelState.AddModelError("UserInitial", $"There is already a User in the system with the same initials. Please enter new Initials using up to three (3) alphanumeric characters.");

                if (_adminService.ValidateNPINumber(userViewModel.NPINumber, user.Id))
                    ModelState.AddModelError("NPINumber", "There is already a User in the system with the same NPI Number. Please enter a different NPI Number ");
                editProfileViewModel.Id = userViewModel.Id;
                editProfileViewModel.FirstName = userViewModel.FirstName;
                editProfileViewModel.LastName = userViewModel.LastName;
                editProfileViewModel.Email = userViewModel.Email;
                editProfileViewModel.UserName = userViewModel.Username;
                editProfileViewModel.CreatedBy = userViewModel.CreatedBy;
                editProfileViewModel.CreatedByName = userViewModel.CreatedByName;
                editProfileViewModel.EnableFive9 = userViewModel.EnableFive9;
                editProfileViewModel.MobilePhone = userViewModel.MobilePhone;
                editProfileViewModel.IsSleep = userViewModel.IsSleep;
                editProfileViewModel.PhoneNumber = userViewModel.OtherPhone;
                editProfileViewModel.CreatedDate = userViewModel.CreatedDate;
                editProfileViewModel.NPINumber = userViewModel.NPINumber;
                editProfileViewModel.CredentialIndex = userViewModel.CredentialIndex;
                editProfileViewModel.UserInitial = userViewModel.UserInitial;
                editProfileViewModel.CaseReviewer = userViewModel.CaseReviewer;
                editProfileViewModel.IsEEG = userViewModel.IsEEG;
                editProfileViewModel.IsStrokeAlert = userViewModel.IsStrokeAlert;
                editProfileViewModel.NHAlert = userViewModel.NHAlert;
                editProfileViewModel.Address_line1 = userViewModel.Address_line1;
                editProfileViewModel.Address_line2 = userViewModel.Address_line2;
                editProfileViewModel.City = userViewModel.City;
                editProfileViewModel.Zip = userViewModel.Zip;
                editProfileViewModel.State_key = userViewModel.State_key;
                editProfileViewModel.PasswordHash = userViewModel.PasswordHash;
                editProfileViewModel.SecurityStamp = userViewModel.SecurityStamp;
                editProfileViewModel.IsActive = userViewModel.isActive;
                editProfileViewModel.IsDisable = userViewModel.isDisable;
                editProfileViewModel.User_Image = fileNames;
                editProfileViewModel.ModifiedByName = userViewModel.ModifiedByName;
                editProfileViewModel.ModifiedBy = userViewModel.ModifiedBy;
                editProfileViewModel.ModifiedDate = userViewModel.ModifiedDate;
                editProfileViewModel.RequirePasswordReset = userViewModel.RequirePasswordReset;
                editProfileViewModel.PasswordExpirationDate = userViewModel.PasswordExpirationDate;
                editProfileViewModel.TwoFactorEnabled = userViewModel.TwoFactorEnabled;
                editProfileViewModel.LockoutEndDateUtc = userViewModel.LockoutEndDateUtc;
                editProfileViewModel.LockoutEnabled = userViewModel.LockoutEnabled;
                editProfileViewModel.AccessFailedCount = userViewModel.AccessFailedCount;
                editProfileViewModel.status_key = userViewModel.status_key;
                editProfileViewModel.status_change_cas_key = userViewModel.status_change_cas_key;
                editProfileViewModel.status_change_date_forAll = userViewModel.status_change_date_forAll;
                editProfileViewModel.IsDeleted = userViewModel.IsDeleted;
                editProfileViewModel.ContractDate = userViewModel.ContractDate;
                editProfileViewModel.APIPassword = userViewModel.APIPassword;
                editProfileViewModel.CredentialCount = userViewModel.CredentialCount;
                editProfileViewModel.status_change_date = userViewModel.status_change_date;
                editProfileViewModel.AddressBlock = userViewModel.AddressBlock;
                editProfileViewModel.PhoneNumberConfirmed = userViewModel.PhoneNumberConfirmed;
                editProfileViewModel.EmailConfirmed = userViewModel.EmailConfirmed;
                editProfileViewModel.Gender = userViewModel.Gender;
                editProfileViewModel.IsTwoFactVerified = userViewModel.IsTwoFactVerified;
                editProfileViewModel.TwoFactVerifyCode = userViewModel.TwoFactVerifyCode;
                editProfileViewModel.CodeExpiryTime = userViewModel.CodeExpiryTime;
                _UserProfileService.EditUserProfile(editProfileViewModel);
                return ShowSuccessMessageOnly("User Profile Successfully Edit..", user);
            }

            catch (Exception ex)
            {

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
            }
            return GetErrorResult(userViewModel);
        }
        [HttpPost]
        public ActionResult Upload(string getid, string getUsername, string getCreatedBy, string getCreatedByName, string getCreatedDate, bool getisActive, string getPasswordHash, string getSecurityStamp)
        {
            AspNetUser editProfileViewModel = new AspNetUser();
            try
            {
                string ImageNames = "";
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    var fileName = DateTime.Now.ToString("yyyymmddMMssfff") + System.IO.Path.GetExtension(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/UserProfileImg/"), fileName);
                    var imagepth = path;
                    file.SaveAs(path);
                    ImageNames += fileName;
                }

                editProfileViewModel.User_Image = ImageNames;
                editProfileViewModel.Id = getid;
                editProfileViewModel.UserName = getUsername;
                editProfileViewModel.CreatedBy = getCreatedBy;
                editProfileViewModel.CreatedByName = getCreatedByName;
                editProfileViewModel.CreatedDate = Convert.ToDateTime(getCreatedDate);
                editProfileViewModel.IsActive = getisActive;
                editProfileViewModel.PasswordHash = getPasswordHash;
                editProfileViewModel.SecurityStamp = getSecurityStamp;


                _UserProfileService.EditUserProfile(editProfileViewModel);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }

            return ShowSuccessMessageOnly("OnBoarding SuccessFuly Added.", true);

        }
        #region ----- Disposable -----
        private bool disposed = false; // to detect redundant calls
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _adminService?.Dispose();
                    _physicianService?.Dispose();
                    _physicianStatus?.Dispose();
                    _physicianStatusLogService?.Dispose();
                    _userLogService?.Dispose();
                    _physicianLicenseService?.Dispose();
                    _facilityPhysicianService?.Dispose();
                    _UserProfileService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion

    }
}