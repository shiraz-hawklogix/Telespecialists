using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TeleSpecialists.BLL.ViewModels.FacilityUser
{
    public class PostFacilityViewModel
    {
        [Required]
        public string UserKey { get; set; }
        [Required]
        public List<Guid> Facilities { get; set; }
    }
    public class PutFacilityViewModel
    {
        public int Id { get; set; }
        [Required]
        public string UserKey { get; set; }
        [Required]
        public Guid Facility { get; set; }
        public string UserFullName { get; set; }
        public string FacilityName { get; set; }
    }
    public class GetFacilityViewModel
    {
        public int Id { get; set; }
        public string UserKey { get; set; }
        public Guid Facility { get; set; }
        public string FacilityName { get; set; }
        public string UserFullName { get; set; }
    }

    public class FacilityViewModel
    {
        public Guid fac_key { get; set; }

        public string fac_name { get; set; }
    }
}
