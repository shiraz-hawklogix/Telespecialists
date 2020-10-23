using System.ComponentModel.DataAnnotations;

namespace TeleSpecialists.BLL.Model
{
    [MetadataType(typeof(entity_noteMetaData))]
    public partial class entity_note
    {  
    }

    public class entity_noteMetaData
    {
        [Required]
        [Display(Name = "Notes")]
        public string etn_notes { get; set; }
        [Required]
        [Display(Name = "Notes Type")]
        public int etn_ntt_key { get; set; }              
    }
}
