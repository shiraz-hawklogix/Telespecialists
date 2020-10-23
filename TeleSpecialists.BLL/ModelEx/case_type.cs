using System.ComponentModel.DataAnnotations;

namespace TeleSpecialists.BLL.Model
{
    [MetadataType(typeof(case_typeMetaData))]
    public partial class case_type
    {
    }

    public class case_typeMetaData
    {
        [Required]
        [Display(Name = "Name")]
        public string ctp_name { get; set; }

        [Required]
        [Display(Name = "Sort Order")]
        public int ctp_sort_order { get; set; }
    }
}
