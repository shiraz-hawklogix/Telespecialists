using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL;


namespace TeleSpecialist.CWH
{
    class Program
    {
        static void Main(string[] args)
        {
            ImportStats();

        }

        private static void ImportStats()
        {
            try
            {
                Console.Title = "Save CWH data";
                new CWHProcesor().DoWork();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
