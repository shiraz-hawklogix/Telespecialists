using System.ComponentModel.DataAnnotations;

namespace TeleSpecialists.BLL.Model
{
    [MetadataType(typeof(application_settingMetaData))]
    public partial class application_setting
    {
    }

    public class application_settingMetaData
    {
        [Display(Name ="Base Url")]
        [RegularExpression(@"^((http|https)://)?([\w-]+\.)+[\w]+(/[\w- ./?]*)?$", ErrorMessage = "Invalid Base Url")]
        public string aps_md_base_url { get; set; }

        [Display(Name = "Token Url")]
        [RegularExpression(@"^((http|https)://)?([\w-]+\.)+[\w]+(/[\w- ./?]*)?$", ErrorMessage = "Invalid Token Url")]
        public string aps_md_token_url { get; set; }

        [Display(Name = "Require Lower Case (a-z)")]
        public bool aps_security_is_lowercase_required { get; set; }

        [Display(Name = "Require Upper Case (A-Z)")]
        public bool aps_security_is_uppercase_required { get; set; }

        [Display(Name = "Require Numeric (0-9)")]
        public bool aps_security_is_number_required { get; set; }

        [Display(Name = "Require Non-Alphanumeric (!,@,#,$,%,^,*,_,+,=)")]
        public bool aps_security_is_non_alphanumeric_required { get; set; }

        [Required]
        [Display(Name = "password length")]
        public decimal aps_security_password_length_value { get; set; }

        [Display(Name = "password age")]
        public decimal aps_security_password_age_value { get; set; }

        [Display(Name = "password history")]
        public decimal aps_security_password_history_value { get; set; }

        [Display(Name = "Require Password Reset on First Login")]
        public bool aps_secuirty_is_reset_password_required { get; set; }

        [Display(Name = "Require Multi-factor Authentication")]
        public bool aps_secuirty_is_multi_factor_authentication_required { get; set; }

        [Required]
        public long? aps_session_timeout_in_minutes { get; set; }
        [Required]
        public long? aps_two_factor_code_expiry_timeout_in_minutes { get; set; }

        [Display(Name = "Enable Case Auto Save")]
        public bool aps_enable_case_auto_save { get; set; }

        [Display(Name = "Case Duplicate Popup Timer (Hours)")]
        [Range(1, int.MaxValue, ErrorMessage = "Only number allowed for hours.")]
        public int aps_duplicate_popup_timer { get; set; }
    }
}
