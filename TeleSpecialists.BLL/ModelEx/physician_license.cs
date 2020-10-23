using System.ComponentModel.DataAnnotations;

namespace TeleSpecialists.BLL.Model
{
    [MetadataType(typeof(physician_licenseMetaData))]
    public partial class physician_license
    {
    }

    public class physician_licenseMetaData
    {
        [Required]
        [Display(Name = "License Number")]
        public string phl_license_number { get; set; }

        [Required]
        [Display(Name ="Start Date")]
        public System.DateTime phl_issued_date { get; set; }
    }
}
