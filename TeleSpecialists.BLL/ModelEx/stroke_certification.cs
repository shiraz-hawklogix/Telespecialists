using System.ComponentModel.DataAnnotations;

namespace TeleSpecialists.BLL.Model
{
    [MetadataType(typeof(stroke_certificationMetaData))]
    public partial class stroke_certification
    {
    }

    public class stroke_certificationMetaData
    {
        [Required]
        [Display(Name = "Name")]
        public string sct_name { get; set; }

        [Required]
        [Display(Name = "Sort Order")]
        public int sct_sort_order { get; set; }
    }
}
