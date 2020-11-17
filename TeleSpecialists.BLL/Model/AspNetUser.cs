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
    
    public partial class AspNetUser
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AspNetUser()
        {
            this.AspNetUserClaims = new HashSet<AspNetUserClaim>();
            this.AspNetUserLogins = new HashSet<AspNetUserLogin>();
            this.AspNetUserRoles = new HashSet<AspNetUserRole>();
            this.AspNetUsers_Log = new HashSet<AspNetUsers_Log>();
            this.AspNetUsers_PasswordReset = new HashSet<AspNetUsers_PasswordReset>();
            this.cases = new HashSet<@case>();
            this.cases1 = new HashSet<@case>();
            this.cases2 = new HashSet<@case>();
            this.case_assign_history = new HashSet<case_assign_history>();
            this.ealert_user_case_type = new HashSet<ealert_user_case_type>();
            this.ealert_user_facility = new HashSet<ealert_user_facility>();
            this.facility_physician = new HashSet<facility_physician>();
            this.physician_license = new HashSet<physician_license>();
            this.physician_status_log = new HashSet<physician_status_log>();
            this.physician_status_snooze = new HashSet<physician_status_snooze>();
            this.post_acute_care = new HashSet<post_acute_care>();
            this.physician_rate = new HashSet<physician_rate>();
            this.user_schedule = new HashSet<user_schedule>();
            this.user_login_verify = new HashSet<user_login_verify>();
            this.user_schedule_nhalert = new HashSet<user_schedule_nhalert>();
            this.user_schedule_sleep = new HashSet<user_schedule_sleep>();
            this.tokens = new HashSet<token>();
            this.mock_case = new HashSet<mock_case>();
            this.mock_case1 = new HashSet<mock_case>();
            this.mock_case2 = new HashSet<mock_case>();
        }
    
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public Nullable<System.DateTime> LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public bool EnableFive9 { get; set; }
        public string MobilePhone { get; set; }
        public string NPINumber { get; set; }
        public string UserInitial { get; set; }
        public string Gender { get; set; }
        public string AddressBlock { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedByName { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public bool CaseReviewer { get; set; }
        public Nullable<int> status_key { get; set; }
        public Nullable<System.DateTime> status_change_date { get; set; }
        public int CredentialCount { get; set; }
        public double CredentialIndex { get; set; }
        public string APISecretKey { get; set; }
        public string APIPassword { get; set; }
        public bool IsEEG { get; set; }
        public bool RequirePasswordReset { get; set; }
        public Nullable<System.DateTime> PasswordExpirationDate { get; set; }
        public Nullable<System.DateTime> ContractDate { get; set; }
        public Nullable<int> status_change_cas_key { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<System.DateTime> status_change_date_forAll { get; set; }
        public bool IsStrokeAlert { get; set; }
        public bool NHAlert { get; set; }
        public bool IsDisable { get; set; }
        public string Address_line1 { get; set; }
        public string Address_line2 { get; set; }
        public string City { get; set; }
        public Nullable<int> State_key { get; set; }
        public string Zip { get; set; }
        public string User_Image { get; set; }
        public Nullable<bool> IsSleep { get; set; }
        public Nullable<bool> IsTwoFactVerified { get; set; }
        public string TwoFactVerifyCode { get; set; }
        public Nullable<System.DateTime> CodeExpiryTime { get; set; }

        public long PhysicianId { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspNetUserRole> AspNetUserRoles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspNetUsers_Log> AspNetUsers_Log { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspNetUsers_PasswordReset> AspNetUsers_PasswordReset { get; set; }
        public virtual physician_status physician_status { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<@case> cases { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<@case> cases1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<@case> cases2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<case_assign_history> case_assign_history { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ealert_user_case_type> ealert_user_case_type { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ealert_user_facility> ealert_user_facility { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<facility_physician> facility_physician { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<physician_license> physician_license { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<physician_status_log> physician_status_log { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<physician_status_snooze> physician_status_snooze { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<post_acute_care> post_acute_care { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<physician_rate> physician_rate { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<user_schedule> user_schedule { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<user_login_verify> user_login_verify { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<user_schedule_nhalert> user_schedule_nhalert { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<user_schedule_sleep> user_schedule_sleep { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<token> tokens { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mock_case> mock_case { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mock_case> mock_case1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mock_case> mock_case2 { get; set; }
    }
}
