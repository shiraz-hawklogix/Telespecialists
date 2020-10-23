using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ViewModels
{

    public class UserLoginVerifyViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public bool? IsTwoFactRememberChecked { get; set; }
        public DateTime? TwoFactVerifyExpiryDate { get; set; }
        public string MachineName { get; set; }
        public string MachineIpAddress { get; set; }
        public string BrowserKey { get; set; }
        public bool? IsLoggedIn { get; set; }
    }

    //
    // Summary:
    //     Possible results from a sign in attempt
    public enum UserLoggedInVerificationStatus
    {
        //
        // Summary:
        //     User was loggin On other Machine
        LogOut = 0,
        //
        // Summary:
        //    Need Two factor Authentication
        IsAuthentationEnabled = 1,
        //
        // Summary:
        //    No Need Two factor Authentication
        IsAuthentationDisabled = 2,
        //
 
    }
}
