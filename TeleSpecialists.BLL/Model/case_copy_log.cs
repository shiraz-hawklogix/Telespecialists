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
    
    public partial class case_copy_log
    {
        public int cpy_key { get; set; }
        public string cpy_source_timezone { get; set; }
        public Nullable<System.DateTime> cpy_source_time { get; set; }
        public Nullable<int> cpy_target_timezone_offset { get; set; }
        public string cpy_target_timezone { get; set; }
        public Nullable<System.DateTime> cpy_target_time { get; set; }
        public string cpy_five9_original_stamp_time { get; set; }
        public Nullable<int> cpy_case_key { get; set; }
        public Nullable<System.Guid> cpy_fac_key { get; set; }
        public string cpy_page_url { get; set; }
        public string cpy_copied_text { get; set; }
        public string cpy_call_id { get; set; }
        public string cpy_fac_name { get; set; }
        public string cpy_user_agent { get; set; }
        public string cpy_browser_name { get; set; }
        public string cpy_created_by { get; set; }
        public string cpy_created_by_name { get; set; }
        public System.DateTime cpy_created_date_est { get; set; }
        public bool cpy_is_info_refreshed { get; set; }
    }
}
