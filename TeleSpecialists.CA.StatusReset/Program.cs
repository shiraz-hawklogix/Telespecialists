using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.CA.StatusAvailable
{
    class Program
    {
        static void Main(string[] args)
        {
            ResetOnSchedulePhysicians(args);
        }

        private static void ResetOnSchedulePhysicians(string[] args)
        {
            //if (args.Count() != 1)
            //{
            //    Console.ForegroundColor = ConsoleColor.Red;
            //    Console.WriteLine("USAGE:");
            //    Console.WriteLine("      TeleSpecialists.CA /StatusReset");

            //    Console.WriteLine("Something like following is a valid usage:");
            //    Console.WriteLine("TeleSpecialists.CA /StatusReset >>> Run process to reset Physician Status");

            //    Console.ForegroundColor = ConsoleColor.White;
            //    Environment.Exit(0);
            //}

            Console.Title = "Physician Status Available";
            new PhysicianStatusAvailableProcess().StatusAvailableProcess();
        }

    }
}
