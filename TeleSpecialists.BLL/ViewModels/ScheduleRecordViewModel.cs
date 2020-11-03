using System;

namespace TeleSpecialists.BLL.ViewModels
{
    public class ScheduleRecordViewModel
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsAllDay { get; set; }
        public string Description { get; set; }
        public string TitleBig { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string UserInitial { get; set; }
        public decimal? Rate { get; set; }
        public int? ShiftId { get; set; }
        public decimal? PhyIndexRate { get; set; }
        public string Title { get; set; }
        public string _scheduleData { get; set; }
        public string _start { get; set; }
        public string _end { get; set; }
        public decimal? TotalIndex { get; set; }
        public bool? IsPublish { get; set; }
        public bool? isFlag { get; set; }
        public int? isFlagMonth { get; set; }
        public int? isFlagDay { get; set; }
    }
    public class FacilityList
    {
        public string facility_name { get; set; }
        public int physcian_count { get; set; }
    }

    //public class FacilityListDayWise
    //{
    //    public DateTime date { get; set; }
    //    public DateTime Start_time { get; set; }
    //    public DateTime End_time { get; set; }
    //    public int Physcian_count { get; set; }
    //}

    public class FacilityListDayWise
    {
        public decimal date { get; set; }
        public int Physcian_count { get; set; }
    }
}
