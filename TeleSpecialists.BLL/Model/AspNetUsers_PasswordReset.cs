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
    
    public partial class AspNetUsers_PasswordReset
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public Nullable<System.DateTime> ExpirationTime { get; set; }
        public string UserName { get; set; }
        public string AspNetUserId { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedByName { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
    }
}
