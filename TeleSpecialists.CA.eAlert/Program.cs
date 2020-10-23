using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.CA.eAlert
{
    class Program
    {
        static void Main(string[] args)
        {
            new eAlertProcess().eAlertResend();     
            new eAlertProcess().SendEmailOfeAlerts();
        }
    }
}
