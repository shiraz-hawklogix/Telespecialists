using System.ComponentModel.DataAnnotations;

namespace TeleSpecialists.BLL.Model
{
    [MetadataType(typeof(facilityMetaData))]
    public partial class facility
    {
    }
    public class facilityMetaData
    {
        [Required]
        [Display(Name = "Name")]
        public string fac_name { get; set; }
           
        //[RegularExpression(@"^\d{5}-_{4}|\d{5}(-\d{4})?$", ErrorMessage = "Invalid Zip. Please enter valid zip code.")]
        [MaxLength(10)]
        [DataType(DataType.PostalCode)]
        [Display(Name = "Zip code")]
        public string fac_zip { get; set; }

        [Required]
        [Display(Name ="Time Zone")]
        public string fac_timezone { get; set; }

        public facility_contract facility_contract { get; set; }
    }
}
