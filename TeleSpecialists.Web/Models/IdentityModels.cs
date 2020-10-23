using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TeleSpecialists.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser<string, IdentityUserLogin, MyUserRole, IdentityUserClaim>
    {
        public ApplicationUser()
        {
            Id = Guid.NewGuid().ToString();
            IsActive = true;
            IsDisable = false;
             CredentialIndex = 0;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDisable { get; set; }
        #region Shiraz Code     
        public string Address_line1 { get; set; }
        public string Address_line2 { get; set; }
        public string City { get; set; }
        public int? State_key { get; set; }
        public string Zip { get; set; }
        public string User_Image { get; set; }
        #endregion
        public bool EnableFive9 { get; set; }     
        public bool CaseReviewer { get; set; }
        public string MobilePhone { get; set; }

        public string NPINumber { get; set; }
        public string UserInitial { get; set; }

        public int? status_key { get; set; }
        public DateTime? status_change_date { get; set; }

        public double CredentialIndex { get; set; }

        public string CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public System.DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }
        public string ModifiedByName { get; set; }
        public System.DateTime? ModifiedDate { get; set; }

        public string APISecretKey { get; set; }
        public string APIPassword { get; set; }

        public Nullable<bool> IsEEG { get; set; } 
        public Nullable<bool> IsStrokeAlert { get; set; }

        [DefaultValue(false)]
        public bool? IsTwoFactVerified { get; set; }
        public string TwoFactVerifyCode { get; set; }
        public DateTime? CodeExpiryTime { get; set; }

        public Nullable<bool> NHAlert { get; set; }
        public bool IsSleep { get; set; }
        public bool RequirePasswordReset { get; set; }      

        public DateTime? PasswordExpirationDate { get; set; }
        public Nullable<int> status_change_cas_key { get; set; }
        public Nullable<System.DateTime> status_change_date_forAll { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<System.DateTime> ContractDate { get; set; }
        public int CredentialCount { get; set; }
        public string AddressBlock { get; set; }
        public string Gender { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public string FullName
        {
            get
            {

                string FullName = "";

                if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName)) FullName = FirstName + " " + LastName;
                else if (!string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName)) FullName = FirstName;
                else if (string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName)) FullName = LastName;
                else FullName = this.UserName;

                return FullName;
            }
        }
    }


    [MetadataType(typeof(MetaData))]
    public class ApplicationRole : IdentityRole<string, MyUserRole>
    {
        public ApplicationRole()
        {
            Id = Guid.NewGuid().ToString();
            Discriminator = "ApplicationRole";
        }

        public string Description { get; set; }
        public string Discriminator { get; set; }
        public class MetaData
        {
            [Required(ErrorMessage = "*Role Name is Required.", AllowEmptyStrings = false)]
            public string Name { get; set; }
        }
    }

    public class MyUserRole : IdentityUserRole {
        public MyUserRole()
        {           
            UserRoleId = Guid.NewGuid().ToString();
        }

        [Key]
        public string UserRoleId { get; set; }    

    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserLogin, MyUserRole, IdentityUserClaim>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        //: base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //override the key properties and define that UserRoleId is the key for extended table(MyUserRole -> AspnetUserRoles)
            modelBuilder.Entity<MyUserRole>().HasKey(hk => new { hk.UserRoleId });

            // TODO: attach sp with table CUD operations (explore)
            //modelBuilder.Entity<MyUserRole>().HasKey(hk => new { hk.UserRoleId }).MapToStoredProcedures(p => p.Delete(sp => sp.HasName("sp_Identity_RemoveUserRole").Parameter(pm => pm.UserRoleId, "UserRoleId")));
        }
    }
}
