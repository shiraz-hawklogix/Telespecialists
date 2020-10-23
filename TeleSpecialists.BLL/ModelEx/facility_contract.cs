using System.ComponentModel.DataAnnotations;

namespace TeleSpecialists.BLL.Model
{

    [MetadataType(typeof(facility_contractMetaData))]
    public partial class facility_contract
    {
        public string fct_selected_services { get; set; }
    }

    public class facility_contractMetaData
    {
        [Display(Name = "Start Date")]
        public System.DateTime fct_start_date { get; set; }
      
        [Display(Name = "End Date")]
        public System.DateTime fct_end_date { get; set; }
        
        [Required]
        [Display(Name = "Service Type")]
        public string fct_selected_services { get; set; }
        [Required]
        [Display(Name = "Coverage Type")]
        public int fct_cvr_key { get; set; }
    }
}
