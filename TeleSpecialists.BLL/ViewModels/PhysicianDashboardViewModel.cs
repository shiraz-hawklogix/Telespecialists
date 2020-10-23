using System;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.ViewModels
{
    public class PhysicianDashboardViewModel
    {
        public AspNetUser physician { get; set; }         
        public TimeSpan? ThreshholdTime { get; set; }
        public int? ElapsedTime { get; set; } // just added so we are able to do sorting on Elasped Time       
    }
}
