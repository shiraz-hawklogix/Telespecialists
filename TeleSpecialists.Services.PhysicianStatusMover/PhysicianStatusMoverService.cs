using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.Services.PhysicianStatusMover
{
    using TeleSpecialists.BLL.Process;

    public partial class PhysicianStatusMoverService : ServiceBase
    {
        private readonly PhysicianStatusProcessor _objService;

        public PhysicianStatusMoverService()
        {
            InitializeComponent();
            _objService = new PhysicianStatusProcessor();
        }

        protected override void OnStart(string[] args)
        {
            _objService.StartService();
        }

        protected override void OnStop()
        {
            _objService.StopService();
        }
    }
}
