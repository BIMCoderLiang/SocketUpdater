using System.ServiceProcess;

namespace UpdaterService
{
    public partial class SocketService : ServiceBase
    {
        public SocketService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ServerSocket.StartServer("127.0.0.1", 8885, 100);
        }

        protected override void OnStop()
        {
        }
    }
}
