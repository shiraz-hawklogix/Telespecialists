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
    
    public partial class cwh_data
    {
        public int cwh_key { get; set; }
        public System.Guid cwh_fac_id { get; set; }
        public string cwh_fac_name { get; set; }
        public Nullable<double> cwh_totalcwh { get; set; }
        public Nullable<double> cwh_month_wise_cwh { get; set; }
        public Nullable<System.DateTime> cwh_date { get; set; }
        public string cwh_modified_by { get; set; }
        public Nullable<System.DateTime> cwh_modified_date { get; set; }
    }
}
