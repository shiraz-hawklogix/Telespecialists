using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace TeleSpecialists.Web.API.Models
{
    public class OwinAuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public OwinAuthDbContext() : base("OwinAuthDbContext")
        {
        }
    }


    public class TelecareDbContext : DbContext
    {
        public TelecareDbContext() : base("name=OwinAuthDbContext")
        {
        }
    }
}