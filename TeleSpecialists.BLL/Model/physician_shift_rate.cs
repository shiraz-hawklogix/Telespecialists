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
    
    public partial class physician_shift_rate
    {
        public int psr_key { get; set; }
        public string psr_phy_key { get; set; }
        public Nullable<int> psr_shift { get; set; }
        public string psr_shift_name { get; set; }
        public Nullable<decimal> psr_rate { get; set; }
        public string psr_created_by { get; set; }
        public Nullable<System.DateTime> psr_created_date { get; set; }
        public string psr_created_by_name { get; set; }
        public string psr_modified_by { get; set; }
        public Nullable<System.DateTime> psr_modified_date { get; set; }
        public string psr_modified_by_name { get; set; }
        public Nullable<System.DateTime> psr_start_date { get; set; }
        public Nullable<System.DateTime> psr_end_date { get; set; }
    }
}
