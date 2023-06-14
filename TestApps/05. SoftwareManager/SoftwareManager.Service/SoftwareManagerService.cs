#region using
using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks; 
#endregion

namespace SoftwareManager.Service
{
    public partial class SoftwareManagerService : ServiceBase
    {
        private Process InstallProcess;
        private Process UninstallProcess;
        private System.Timers.Timer timer;

        public SoftwareManagerService()
        {
            using (var sec = this.GetCodeSection())
            {
                InitializeComponent();
            }
        }

        protected override void OnStart(string[] args)
        {
            using (var sec = this.GetCodeSection(new { args = args.GetLogString() }))
            {

            }
        }

        protected override void OnStop()
        {
            using (var sec = this.GetCodeSection())
            {
            }
        }
    }
}
