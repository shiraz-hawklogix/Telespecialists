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
    
    public partial class component
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public component()
        {
            this.component_access = new HashSet<component_access>();
        }
    
        public int com_key { get; set; }
        public Nullable<int> com_parentcomponentid { get; set; }
        public string com_module_name { get; set; }
        public string com_page_url { get; set; }
        public string com_page_name { get; set; }
        public string com_page_title { get; set; }
        public string com_page_description { get; set; }
        public string com_form_id { get; set; }
        public bool com_status { get; set; }
        public string com_addedby { get; set; }
        public Nullable<System.DateTime> com_addedon { get; set; }
        public string com_modifiedby { get; set; }
        public Nullable<System.DateTime> com_modifiedon { get; set; }
        public Nullable<int> com_sortorder { get; set; }
        public string com_moduleimage { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<component_access> component_access { get; set; }
    }
}