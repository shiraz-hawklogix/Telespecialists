using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TeleSpecialists.BLL.ViewModels.FacilityUser
{
   public class PostCaeTypeViewModel
    {
        [Required]
        public string UserKey { get; set; }
        [Required]
        public List<int> CaseTypes { get; set; }
    }
    public class PutCaeTypeViewModel
    {
        public int Id { get; set; }
        [Required]
        public string UserKey { get; set; }
        [Required]
        public int CaseTypeKey { get; set; }
        public string UserFullName { get; set; }
        public string CaseTypeName { get; set; }
    }
    public class GetCaeTypeViewModel
    {
        public int Id { get; set; }
        public string UserKey { get; set; }
        public int CaseTypeKey { get; set; }
        public int? CaseUCLKey { get; set; }
        public string UserFullName { get; set; }
        public string CaseTypeName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
