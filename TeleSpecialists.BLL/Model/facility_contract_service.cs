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
    
    public partial class facility_contract_service
    {
        public int fcs_key { get; set; }
        public int fcs_srv_key { get; set; }
        public System.Guid fcs_fct_key { get; set; }
        public System.DateTime fcs_created_date { get; set; }
        public string fcs_created_by { get; set; }
        public string fcs_created_by_name { get; set; }
        public bool fcs_is_active { get; set; }
    
        public virtual facility_contract facility_contract { get; set; }
    }
}
