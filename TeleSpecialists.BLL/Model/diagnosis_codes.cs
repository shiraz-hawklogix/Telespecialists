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
    
    public partial class diagnosis_codes
    {
        public int code_id { get; set; }
        public Nullable<int> diag_cat_parent_id { get; set; }
        public string icd_code { get; set; }
        public string icd_code_title { get; set; }
        public string icd_code_description { get; set; }
        public string icd_code_impression { get; set; }
        public Nullable<int> sort_order { get; set; }
        public Nullable<System.DateTime> date_added { get; set; }
        public Nullable<System.DateTime> date_updated { get; set; }
        public Nullable<bool> is_active { get; set; }
    }
}
