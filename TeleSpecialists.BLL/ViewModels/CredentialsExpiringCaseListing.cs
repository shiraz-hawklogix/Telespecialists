using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ViewModels
{
    public class CredentialsExpiringCaseListing
    {
        public List<CredentialsExpiringCase> CredentialsCases { get; set; } = new List<CredentialsExpiringCase>();
    }
    public class CredentialsExpiringCase
    {
        public int Fac_Key { get; set; }
        public string PhysicianName { get; set; }
        public string FacilityName { get; set; }
        public string EndDate { get; set; }
        public int TotalRecords { get; set; }
    }
}
