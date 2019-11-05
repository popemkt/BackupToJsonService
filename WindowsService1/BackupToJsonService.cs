using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Timers;

namespace WindowsService1
{
    public partial class BackupToJsonService : ServiceBase
    {
        private Timer timer;
        private int Interval;
        #region ServiceStatus
        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);
        #endregion
        public BackupToJsonService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Interval = int.Parse(ConfigurationManager.AppSettings.Get("Interval"));
            var serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(ServiceHandle, ref serviceStatus);
            timer = new Timer
            {
                Interval = this.Interval
            };
            timer.Elapsed += new ElapsedEventHandler(OnTimer);
            timer.Start();
            LoggingService.WriteToLog("Service Start");
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            IEnumerable<Person> personList = DBService.ReadDb();
            if (personList != null && personList.Any())
            {
                bool result = FileService.WriteToJson(personList);
                if (result)
                {
                    bool updateResult = DBService.UpdateDb(personList);
                    if (updateResult)
                    {
                        LoggingService.WriteToLog("Write operation successful!");
                    }
                    else
                    {
                        LoggingService.WriteToLog("Update database failed!");
                    }
                }
                else
                {
                    LoggingService.WriteToLog("Nothing written!");
                }
            }
        }

        protected override void OnStop()
        {
            var serviceStatus = new ServiceStatus();
            LoggingService.WriteToLog("Service Stopped");
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }
        protected override void OnPause()
        {
            var serviceStatus = new ServiceStatus();
            LoggingService.WriteToLog("Service Paused");
            serviceStatus.dwCurrentState = ServiceState.SERVICE_PAUSED;
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }
        protected override void OnContinue()
        {
            var serviceStatus = new ServiceStatus();
            LoggingService.WriteToLog("Service Continued");
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            if (timer == null)
            {
                timer = new Timer
                {
                    Interval = this.Interval
                };
                timer.Elapsed += new ElapsedEventHandler(OnTimer);
                timer.Start();
            }
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }

    }
}
