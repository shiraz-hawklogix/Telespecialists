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
    
    public partial class user_access
    {
        public int user_key { get; set; }
        public string user_role_key { get; set; }
        public string user_id { get; set; }
        public Nullable<int> user_com_key { get; set; }
        public Nullable<bool> user_isAllowed { get; set; }
        public Nullable<System.DateTime> user_createddate { get; set; }
        public string user_createdBy { get; set; }
        public Nullable<System.DateTime> user_updateddate { get; set; }
        public string user_updatedBy { get; set; }
    }
}
