using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.CA
{
    class Program
    {
        static void Main(string[] args)
        {
            //new FTPTest().Test();

            if (args.Length == 0)
            {
                string strServiceMode = System.Configuration.ConfigurationManager.AppSettings.Get("ServiceMode");

                if (string.IsNullOrEmpty(strServiceMode))
                {
                    Help();

                    return;
                }
                else
                {
                    Array.Resize(ref args, args.Length + 1);
                    args[args.Length - 1] = strServiceMode;
                    //args.Append(strServiceMode);
                }
            }

            //Console.Clear();

            //while (true) 
            {
                try
                {
                    switch (args[0].ToLower())
                    {
                        case "/?":
                        case "?":
                        case "/all":
                        case "all":
                            break;
                        case "mdstaffimport":
                        case "/mdstaffimport":
                            ImportStats(args);
                            break;
                        case "hospital":
                        case "/hospital":
                            ImportHospitals(args);
                            break;
                        case "statusmover":
                        case "/statusmover":
                            PhysicianStatusMover(args);
                            break;
                        case "statusreset":
                        case "/statusreset":
                            ResetOnSchedulePhysicians(args);
                            break;
                        case "unschedulereset":
                        case "/unschedulereset":
                            ResetUnSchedulePhysicians(args);
                            break;
                        case "rapids":
                        case "/rapids":
                            ImportRapids(args);
                            break;
                        default:
                            Help();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("Press any key to continue .....");

                    Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

        }

        private static void Help()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("Error: unrecognized or incomplete command line.");
            Console.WriteLine("");
            Console.WriteLine("USAGE:");
            Console.WriteLine("TeleSpecialists.CA              [/? | /all | /Options]");


            Console.WriteLine("where");
            Console.WriteLine("     Options:");
            Console.WriteLine("     /?                   Display this help message.");
            Console.WriteLine("     /all                 Display full configuration information.");
            Console.WriteLine("     /MDStaff             Run Import process for MD Staff API.");
            Console.WriteLine("     /StatusMover         Run process to update Physician Status.");

            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void PhysicianStatusMover(string[] args)
        {
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

            Console.Title = "Physician Status Mover";

            new BLL.Process.PhysicianStatusProcessor().MovePhysicianStatuses();
        }

        private static void ResetOnSchedulePhysicians(string[] args)
        {
            if (args.Count() != 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("USAGE:");
                Console.WriteLine("      TeleSpecialists.CA /StatusReset");

                Console.WriteLine("Something like following is a valid usage:");
                Console.WriteLine("TeleSpecialists.CA /StatusReset >>> Run process to reset Physician Status");

                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(0);
            }

            Console.Title = "Physician Status Reset";

            new BLL.Process.PhysicianStatusProcessor().ResetOnSchedulePhysicians();
        }

        private static void ResetUnSchedulePhysicians(string[] args)
        {
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

            Console.Title = "Physician Schedule Reset";

            new BLL.Process.PhysicianStatusProcessor().ResetUnSchedulePhysicians();
        }

        private static void ImportStats(string[] args)
        {
            try
            {
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

                Console.Title = "Import MD Staff Data";

                string requestId = Guid.NewGuid().ToString();


                new BLL.Process.MDStaffProcessor().DoWork(requestId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void ImportHospitals(string[] args)
        {
            if (args.Count() != 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("USAGE:");
                Console.WriteLine("      TeleSpecialists.CA /hospital");

                Console.WriteLine("Something like following is a valid usage:");
                Console.WriteLine("TeleSpecialists.CA /hospital >>> Import MD Staff API data of hospitals");

                Console.ForegroundColor = ConsoleColor.White;
                Environment.Exit(0);
            }

            Console.Title = "Import MD Staff Hospitals";

            string requestId = Guid.NewGuid().ToString();


            new BLL.Process.MDStaff.MDStaffImport().GetAllHospitials(requestId, Guid.Empty.ToString(), "Facility Import Service").Wait();
        }

        private static void ImportRapids(string[] args)
        {
            try
            {
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

                Console.Title = "Import Rapids";

                string requestId = Guid.NewGuid().ToString();


                new BLL.Process.RapidsProcessor().DoWork();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
