//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TeleSpecialists.BLL.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class facility_questionnaire_contact_designation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public facility_questionnaire_contact_designation()
        {
            this.facility_questionnaire_contact = new HashSet<facility_questionnaire_contact>();
        }
    
        public int fqd_key { get; set; }
        public string fqd_name { get; set; }
        public bool fqd_is_active { get; set; }
        public Nullable<int> fqd_sort_order { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<facility_questionnaire_contact> facility_questionnaire_contact { get; set; }
    }
}
