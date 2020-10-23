using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.CA.StatusMover
{
    class Program
    {
        static void Main(string[] args)
        {
            PhysicianStatusMover(args);
        }

        private static void PhysicianStatusMover(string[] args)
        {
            /*
            if (args.Count() != 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("USAGE:");
                Console.WriteLine("      TeleSpecialists.CA /StatusMover");

                Console.WriteLine("Something like following is a valid usage:");
                Console.WriteLine("TeleSpecialists.CA /StatusMover >>> Run process to update Physician Status");

                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(0);
            }
            */

            Console.Title = "Physician Status Mover";

            new PhysicianStatusMover().MovePhysicianStatuses();            
        }
    }
}
