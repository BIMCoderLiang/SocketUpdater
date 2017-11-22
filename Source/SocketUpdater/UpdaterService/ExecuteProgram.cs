using System.ServiceProcess;

namespace UpdaterService
{
    static class ExecuteProgram
    {
        static void Main()
        {
            var servicesToRun = new ServiceBase[]
            {
                new SocketService()
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
