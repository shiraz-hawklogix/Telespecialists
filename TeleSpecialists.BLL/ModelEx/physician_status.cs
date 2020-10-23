using System.ComponentModel.DataAnnotations;
namespace TeleSpecialists.BLL.Model
{
    [MetadataType(typeof(physician_statusMetaData))]
    partial class physician_status
    {
    }
    public class physician_statusMetaData
    {
        [Required]
        [Display(Name = "Name")]
        public string phs_name { get; set; }
        [Required]
        [Display(Name = "Color Code")]
        public string phs_color_code { get; set; }
        [Required]
        [Display(Name = "Sort Order")]
        public int phs_sort_order { get; set; }
    }
}
