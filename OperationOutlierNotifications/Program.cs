using System;
using Telespecialists.PendingCasesNotification;

namespace OperationOutlierNotifications
{
    class Program
    {
        static void Main(string[] args)
        {
            SendOperationOutliersNotifications();
        }

        private static void SendOperationOutliersNotifications()
        {

            Console.Title = "Send Operation Outliers Notifications";
            new OperationOutliersNotifications().DoWork();
        }
    }
}
