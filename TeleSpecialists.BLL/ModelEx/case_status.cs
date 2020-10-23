using System.ComponentModel.DataAnnotations;

namespace TeleSpecialists.BLL.Model
{
    [MetadataType(typeof(case_statusMetaData))]
    public partial class case_status
    {
    }

    public class case_statusMetaData
    {
        [Required]
        [Display(Name = "Name")]
        public string cst_name { get; set; }

        [Required]
        [Display(Name = "Sort Order")]
        public int cst_sort_order { get; set; }
    }
}
