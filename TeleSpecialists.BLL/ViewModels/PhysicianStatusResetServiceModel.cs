using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.ViewModels
{
    public class PhysicianStatusResetServiceModel
    {
        public AspNetUser physician { get; set; }
        public user_schedule schedule { get; set; }
    }

    public class NHPhysicianStatusResetServiceModel
    {
        public AspNetUser physician { get; set; }
        public user_schedule_nhalert schedule { get; set; }
    }

    
}
