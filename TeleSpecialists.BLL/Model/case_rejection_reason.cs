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
    
    public partial class case_rejection_reason
    {
        public int crr_key { get; set; }
        public string crr_reason { get; set; }
        public bool crr_troubleshoot { get; set; }
        public Nullable<int> crr_parent_key { get; set; }
        public string crr_users { get; set; }
        public Nullable<System.DateTime> crr_created_on { get; set; }
        public string crr_created_by { get; set; }
        public Nullable<System.DateTime> crr_modified_on { get; set; }
        public string crr_modified_by { get; set; }
    }
}