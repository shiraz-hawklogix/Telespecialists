using System.Collections.Generic;

namespace TeleSpecialists.BLL.ViewModels
{
    public class SchedulerResponseViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string FileId { get; set; }
        public List<string> ParseErrors { get;set; }   
        
        public SchedulerResponseViewModel()
        {
            ParseErrors = new List<string>();
            Success = true;
        }
    }
}
