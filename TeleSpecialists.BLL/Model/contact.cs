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
    
    public partial class contact
    {
        public int cnt_key { get; set; }
        public string cnt_first_name { get; set; }
        public string cnt_last_name { get; set; }
        public string cnt_role { get; set; }
        public string cnt_primary_phone { get; set; }
        public string cnt_mobile_phone { get; set; }
        public string cnt_email { get; set; }
        public bool cnt_is_active { get; set; }
        public System.DateTime cnt_created_date { get; set; }
        public string cnt_created_by { get; set; }
        public Nullable<System.DateTime> cnt_modified_date { get; set; }
        public string cnt_modified_by { get; set; }
        public System.Guid cnt_fac_key { get; set; }
        public string cnt_created_by_name { get; set; }
        public string cnt_modified_by_name { get; set; }
        public Nullable<int> cnt_role_ucd_key { get; set; }
        public bool cnt_is_deleted { get; set; }
        public string cnt_extension { get; set; }
    
        public virtual ucl_data ucl_data { get; set; }
        public virtual facility facility { get; set; }
    }
}
