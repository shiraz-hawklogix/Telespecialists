namespace TeleSpecialists.BLL.ViewModels
{
    public class LoadProgramParticipationViewModel
    {
        public string ProgramName { get; set; }
        public int Submitted { get; set; }
        public int Awarded { get; set; }
    }

    public class LoadMonetaryAwardsViewModel
    {
        public string ProgramName { get; set; }
        public decimal Monetary { get; set; }
        public decimal Extended { get; set; }
    }
    public class StrokeAlertChartViewModel
    {
        public int Hour { get; set; }
        public int Count { get; set; }
        public int PhysicianBlastCount { get; set; }
        public int NavigatorBlastCount { get; set; }
        public int STATCount { get; set; }
    }


}
