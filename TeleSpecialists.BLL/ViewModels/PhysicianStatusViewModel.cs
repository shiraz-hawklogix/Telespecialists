using System;

namespace TeleSpecialists.BLL.ViewModels
{
    public class PhysicianStatusViewModel
    {
        //public string Id { get; set; }
        //public string Name { get; set; }
        //public bool IsAvailableStatus { get; set; }
        //public string StatusChangeDate { get; set; }
        //public string CreatedDate { get; set; }
        //public bool isScheduled { get; set; }
        //public double CredentialIndex { get; set; }
        //public string StatusName { get; set; }
        //public string StatusColorCode { get; set; }
        //public string MobilePhone { get; set; }
        //public string PhoneNumber { get; set; }
        //public string ElapsedTime { get; set; }


        public int ID { get; set; }
        public int scheduleExist { get; set; }
        public Guid AspNetUser_Id { get; set; }
        public int AspNetUser_status_key { get; set; }
        public Nullable<DateTime> AspNetUser_status_change_date { get; set; }
        public double AspNetUser_CredentialIndex { get; set; }
        public string AspNetUser_FirstName { get; set; }
        public string AspNetUser_LastName { get; set; }
        public string FullName { get; set;}
        public Nullable<DateTime> AspNetUser_CreatedDate { get; set; }
        public string AspNetUser_PhoneNumber { get; set; }
        public string AspNetUser_MobilePhone { get; set; }
        public Nullable<DateTime> fap_start_date { get; set; }
        public Nullable<DateTime> currentSchedule_uss_time_to_calc { get; set; }
        public Guid AspNetUserRoles_RoleId { get; set; }
        public int phs_assignment_priority { get; set; }
        public int AspNetUser_physician_status { get; set; }
        public string phs_color_code { get; set; }
        public string phs_name { get; set; }
        public string ElapsedTime { get; set; }
        public bool FinalSorted { get; set; }
    }
}
