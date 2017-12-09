using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace UpdaterService
{
    public partial class SocketService : ServiceBase
    {
        Thread threadforwork = null;

        public SocketService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (threadforwork == null)
            {
                threadforwork = new Thread(p =>
                {
                    try
                    {
                        ServerSocket.StartServer(8885, 100);
                    }
                    catch (Exception ex)
                    {
                        var path = $"{AppDomain.CurrentDomain.BaseDirectory}\\ServiceStartLog.txt";
                        File.AppendAllText(path, ex.Message);
                    }
                });
                threadforwork.IsBackground = true;
                threadforwork.Start();
            }
        }

        protected override void OnStop()
        {
            if (threadforwork?.ThreadState == ThreadState.Running)
            {
                threadforwork.Abort();
            }
        }
    }
}
