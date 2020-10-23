using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.CA.Rapids
{
    class Program
    {
        static void Main(string[] args)
        {
            ImportRapids(args);
        }

        private static void ImportRapids(string[] args)
        {
            try
            {
                /*
                if (args.Count() != 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("USAGE:");
                    Console.WriteLine("      TeleSpecialists.CA /Rapids");

                    Console.WriteLine("Something like following is a valid usage:");
                    Console.WriteLine("TeleSpecialists.CA /Rapids >>> Import Rapids Emails data in system");

                    Console.ForegroundColor = ConsoleColor.White;
                    Environment.Exit(0);
                }
                */
                Console.Title = "Import Rapids";

                new BLL.Process.RapidsProcessor().DoWork();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
