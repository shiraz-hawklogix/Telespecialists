using System;


namespace Telespecialists.PendingCasesNotification
{
    class Program
    {
        static void Main(string[] args)
        {
            SendPendingCasesNotifications();
        }


        private static void SendPendingCasesNotifications()
        {

            Console.Title = "Pending Cases Notifications";
            new PendingCasesNotification().DoWork();
        }
    }
}
