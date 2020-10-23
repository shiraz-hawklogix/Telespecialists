using System.Threading;
using TeleSpecialists.BLL.Helpers;

namespace TeleSpecialists.BLL.Process
{
    public class ProcessorBase
    {
        protected readonly EventLogger _logger;
        protected readonly ManualResetEvent _shutdownEvent;
        protected readonly int _sleepInterval = 5;
        protected Thread _thread;    
        protected string _serviceName = "TeleSpecialists Service";    

        public ProcessorBase()
        {
            _logger = new EventLogger();
            _shutdownEvent = new ManualResetEvent(false);
            // get service sleep interval
            int.TryParse(System.Configuration.ConfigurationManager.AppSettings.Get("SleepInterval"), out _sleepInterval);
        }
    }
}
