using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleSpecialists.Web.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string APISecretKey { get; set; }
        public string APIPassword { get; set; }
    }
}