using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;

namespace HamachiRestarter
{
    partial class RestartService : ServiceBase
    {
        private const string SERVICE_NAME = "Hamachi2Svc";
        private const int RESTART_INTERVAL_HOURS = 4;
        private const int MAX_DOWN_TIME_HOURS = 1;

        private readonly object _syncObjet = new object();
        private TimeSpan _restartInterval;
        private DateTime _lastRestart;
        private DateTime _lastRestartAttempt;
        private bool _lastRestartAttemptFailed;
        private Timer _timer;



        public RestartService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _lastRestartAttempt = _lastRestart = DateTime.UtcNow;
            _restartInterval = TimeSpan.FromMinutes(2);

            _timer = new Timer(TimerCallback, null, _restartInterval, _restartInterval);

            // minimize memory
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, (UIntPtr)0xFFFFFFFF, (UIntPtr)0xFFFFFFFF);
        }

        private void TimerCallback(object state)
        {
            lock (_syncObjet)
            {
                var now = DateTime.UtcNow;
                if (_lastRestartAttemptFailed && (now - _lastRestart).TotalHours > MAX_DOWN_TIME_HOURS)
                {
                    // we've been trying to restart for an hour without success; reset the machine
                    SystemHelper.RestartWindows();
                    return;
                }

                if(_lastRestartAttemptFailed || (now - _lastRestartAttempt).TotalHours > RESTART_INTERVAL_HOURS)
                {
                    try
                    {
                        _lastRestartAttempt = now;
                        ServiceHelper.Start(SERVICE_NAME, restartIfRunning: true);
                        _lastRestart = now;
                        _lastRestartAttemptFailed = false;
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("Unable to restart: " + exception.Message);
                        _lastRestartAttemptFailed = true;
                    }
                }
            }
        }

        protected override void OnStop()
        {
            lock (_syncObjet)
            {
                _timer.Dispose();
            }
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetProcessWorkingSetSize(IntPtr process,
            UIntPtr minimumWorkingSetSize, UIntPtr maximumWorkingSetSize);
    }
}
