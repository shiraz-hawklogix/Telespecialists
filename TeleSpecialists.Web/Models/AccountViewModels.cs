using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using TeleSpecialists.BLL.Helpers;

namespace TeleSpecialists.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        //[Required]
        //[Display(Name = "Email")]
        //[EmailAddress]
        //public string Email { get; set; }

        public string UserId { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        [Display(Name = "Pin code")]
        public string TwoFactVerifyCode { get; set; }

        [Display(Name = "Remember me for 30 days")]
        public bool RememberMeSms { get; set; }

        public string isAuthenticationChecked { get; set; }

        public string isLogout { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }
    }

    public class RegisterViewModel
    {
        public string Id { get; set; }
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^[ A-Za-z0-9!@#$%^*_+=]*$", ErrorMessage = "These (&,~,`,(,),-) characters are not allowed.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        public bool isActive { get; set; }
        public bool isDisable { get; set; }

        [Required]
        [Display(Name ="Role")]
        public string Role { get; set; }

        public string Level { get; set; }
        public bool EnableFive9 { get; set; }
        
        public bool CaseReviewer { get; set; }

        [Required]
        [RegularExpression(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$", ErrorMessage = "Invalid phone number. Please enter valid phone number.")]
        public string MobilePhone { get; set; }
        public string OtherPhone { get; set; }

        
        [RegularExpression(@"^\d{10}", ErrorMessage = "Invalid NPI Number")]
        public string NPINumber { get; set; }

        //[Required]
        [StringLength(6)]
        public string UserInitial { get; set; }

        public string APISecretKey { get; set; }

        public string APIPassword { get; set; }
        public bool IsEEG { get; set; } 
        public bool IsStrokeAlert { get; set; }
        public bool NHAlert { get; set; }
        public bool IsSleep { get; set; }
        public bool TwoFactorEnabled { get; set; }
        #region Shiraz Code     
        public string Address_line1 { get; set; }
        public string Address_line2 { get; set; }
        public string City { get; set; }
        public int? State_key { get; set; }
        public string Zip { get; set; }
        #endregion
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Display(Name = "User name")]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public bool isActive { get; set; }
        public bool isDisable { get; set; }

        public string Role { get; set; }
        public string Level { get; set; }
        public bool EnableFive9 { get; set; }

        public bool CaseReviewer { get; set; }
        [Required]
        [RegularExpression(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$", ErrorMessage = "Invalid phone number. Please enter valid phone number.")]
        public string MobilePhone { get; set; }
        public string OtherPhone { get; set; }


        [RegularExpression(@"^\d{10}", ErrorMessage = "Invalid NPI Number")]
        public string NPINumber { get; set; }

        //[Required]
        public string UserInitial { get; set; }

        public string APISecretKey { get; set; }
        public string APIPassword { get; set; }
        public bool IsEEG { get; set; }
        public bool IsStrokeAlert { get; set; }
        public bool NHAlert { get; set; }
        public bool  IsSleep { get; set; }
        public bool LockoutEnabled { get; set; }
        public bool TwoFactorEnabled { get; set; }

        #region Shiraz Code
        public string Address_line1 { get; set; }
        public string Address_line2 { get; set; }
        public string City { get; set; }
        public int? State_key { get; set; }
        public string Zip { get; set; }
        #endregion
    }
    public class EditProfileViewModel 
    {
        public string Id { get; set; }

        [Display(Name = "User name")]
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        public System.DateTime CreatedDate { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public bool isActive { get; set; }
        public bool isDisable { get; set; }

        public string Role { get; set; }
        public string Level { get; set; }
        public bool EnableFive9 { get; set; }

        public bool CaseReviewer { get; set; }
        [Required]
        [RegularExpression(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$", ErrorMessage = "Invalid phone number. Please enter valid phone number.")]
        public string MobilePhone { get; set; }
        public string OtherPhone { get; set; }


        [RegularExpression(@"^\d{10}", ErrorMessage = "Invalid NPI Number")]
        public string NPINumber { get; set; }

        [Required]
        public string UserInitial { get; set; }

        public string APISecretKey { get; set; }
        public string APIPassword { get; set; }
        public bool IsEEG { get; set; }
        public bool IsStrokeAlert { get; set; }
        public bool NHAlert { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        #region Shiraz Code
        public string Address_line1 { get; set; }
        public string Address_line2 { get; set; }
        public string City { get; set; }
        public int? State_key { get; set; }
        public string Zip { get; set; }
        public string  User_Image { get; set; }
        public string ModifiedByName { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public bool RequirePasswordReset { get; set; }
        public Nullable<System.DateTime> PasswordExpirationDate { get; set; }
        public HttpPostedFileBase Imagefile { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public Nullable<System.DateTime> LockoutEndDateUtc { get; set; }
        public Nullable<int> status_key { get; set; }
        public Nullable<int> status_change_cas_key { get; set; }
        public double CredentialIndex { get; set; }
        public Nullable<System.DateTime> status_change_date_forAll { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<System.DateTime> ContractDate { get; set; }
        public int CredentialCount { get; set; }
        public Nullable<System.DateTime> status_change_date { get; set; }
        public string AddressBlock { get; set; }
        public string Gender { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsSleep { get; set; }
        public Nullable<bool> IsTwoFactVerified { get; set; }
        public string TwoFactVerifyCode { get; set; }
        public Nullable<System.DateTime> CodeExpiryTime { get; set; }
        #endregion
    }


    public class UpdateUserViewModel
    {
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "User name")]
        public string Username { get; set; }
        [Required]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^[ A-Za-z0-9!@#$%^*_+=]*$", ErrorMessage = "These (&,~,`,(,),-) characters are not allowed.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


        
        public string APISecretKey { get; set; }        
        public string APIPassword { get; set; }
        public bool IsApiUser { get; set; }

    }

    public class CredentialIndexViewModel
    {
        [Display(Name = "Cl")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [Range(0.00, 100.00, ErrorMessage =  "Invalid Credential Index")]
        public double CredentialIndex { get; set; }

        [Required]
        public string UserId { get; set; }

    }

    public class ChangePassowrdFirstLoginViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string OldPassword { get; set; }

        [Required]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Error! UserName not found. Try to login again.")]
        public string UserName { get; set; }

        public bool IsPasswordExpired { get; set; } = false;
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

  

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }
    }
}

