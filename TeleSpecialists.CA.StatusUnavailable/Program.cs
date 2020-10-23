using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Common.Process;

namespace TeleSpecialists.CA.StatusUnavailable
{
    class Program
    {
        static void Main(string[] args)
        {
            ResetUnSchedulePhysicians(args);
        }

        private static void ResetUnSchedulePhysicians(string[] args)
        {
            /*
            if (args.Count() != 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("USAGE:");
                Console.WriteLine("      TeleSpecialists.CA /ScheduleReset");

                Console.WriteLine("Something like following is a valid usage:");
                Console.WriteLine("TeleSpecialists.CA /ScheduleReset >>> Run process to reset Physician Schedule");

                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(0);
            }
            */
            Console.Title = "Physician Schedule Reset";
            new PhysicianStatusProcessor().ResetUnSchedulePhysicians();
        }

    }
}
