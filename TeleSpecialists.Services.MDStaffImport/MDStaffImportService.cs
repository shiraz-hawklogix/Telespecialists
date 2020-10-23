using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.Services.MDStaffImport
{
    using TeleSpecialists.BLL.Process;

    public partial class MDStaffImportService : ServiceBase
    {
        private readonly MDStaffProcessor _objService;

        public MDStaffImportService()
        {
            InitializeComponent();
            _objService = new MDStaffProcessor();
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
