using System.ComponentModel.DataAnnotations;

namespace TeleSpecialists.BLL.Model
{
    [MetadataType(typeof(contactMetaData))]
    public partial class contact
    {
    }

    public class contactMetaData
    {
        [Display(Name = "Email")]
        [EmailAddress]
        public string cnt_email { get; set; }

        [Display(Name = "Primary Phone")]
        [RegularExpression(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$", ErrorMessage = "Invalid phone number. Please enter valid phone number.")]
        public string cnt_primary_phone { get; set; }

        [Display(Name = "Mobile Phone")]
        [RegularExpression(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$", ErrorMessage = "Invalid phone number. Please enter valid phone number.")]
        public string cnt_mobile_phone { get; set; }
    }
}
