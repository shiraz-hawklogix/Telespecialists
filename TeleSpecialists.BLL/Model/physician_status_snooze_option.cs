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
    
    public partial class physician_status_snooze_option
    {
        public int pso_key { get; set; }
        public string pso_message { get; set; }
        public System.TimeSpan pso_snooze_time { get; set; }
        public bool pso_is_active { get; set; }
        public System.DateTime pso_created_date { get; set; }
        public string pso_created_by { get; set; }
        public string pso_created_by_name { get; set; }
        public Nullable<System.DateTime> pso_modified_date { get; set; }
        public string pso_modified_by { get; set; }
        public string pso_modified_by_name { get; set; }
        public int pso_phs_key { get; set; }
    
        public virtual physician_status physician_status { get; set; }
    }
}
