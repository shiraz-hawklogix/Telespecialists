using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.CA.MDStaff
{
    class Program
    {
        static void Main(string[] args)
        {
            ImportStats(args);
        }

        private static void ImportStats(string[] args)
        {
            try
            {
                /*
                if (args.Count() != 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("USAGE:");
                    Console.WriteLine("      TeleSpecialists.CA /Import");

                    Console.WriteLine("Something like following is a valid usage:");
                    Console.WriteLine("TeleSpecialists.CA /MDStaff >>> Import MD Staff API data");

                    Console.ForegroundColor = ConsoleColor.White;
                    Environment.Exit(0);
                }
                */

                Console.Title = "Import MD Staff Data";

                string requestId = Guid.NewGuid().ToString();

                new BLL.Process.MDStaffProcessor().DoWork(requestId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
