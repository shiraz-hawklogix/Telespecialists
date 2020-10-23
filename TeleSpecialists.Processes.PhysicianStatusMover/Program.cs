using TeleSpecialists.BLL.Service;
using System.Linq;
using System.Data.Entity;
using System;
using System.Collections.Generic;

using TeleSpecialists.BLL.Process;

namespace TeleSpecialists.Processes.PhysicianStatusMover
{
    public class Program
    {
        static void Main(string[] args)
        {
            // to run once
          //  new PhysicianStatusProcessor().ProcessStatus();


            // to run continuously 
            
            new PhysicianStatusProcessor().StartService();

            Console.ReadLine();
            Environment.Exit(0);
            
        }
    }
}
