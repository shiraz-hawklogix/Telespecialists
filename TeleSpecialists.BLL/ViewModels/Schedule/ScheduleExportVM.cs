using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ViewModels.Schedule
{
    public class ScheduleExportVM
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string TitleBig { get; set; }
        public string Description { get; set; }
        public string ScheduleDate { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public bool? IsActive { get; set; }
        public string FullName { get; set; }
        public bool IsAllDay { get; set; }
        public decimal? Rate { get; set; }
        public int? ShiftId { get; set; }
        public double PhyIndexRate { get; set; }
    }
}
