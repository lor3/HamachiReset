using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace HamachiRestarter
{
    class Program
    {
        public static void Main(string[] args)
        {

            if (Environment.UserInteractive)
            {
                var parameter = string.Concat(args);
                switch (parameter)
                {
                    case "--install":
                        ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                        break;
                    case "--uninstall":
                        ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                        break;
                }
            }
            else
            {
                ServiceBase.Run(new ServiceBase[] { new RestartService()  });
            }
        }
    }
}
