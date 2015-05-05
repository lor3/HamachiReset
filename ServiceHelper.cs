using System.Collections.Generic;
using System.Management;
using System.ServiceProcess;
using System.Threading;

namespace HamachiRestarter
{
    /// <summary>
    /// Static helpers for querying and modifying system services.
    /// </summary>
    public static class ServiceHelper
    {
        /// <summary>
        /// Gets the list of installed services.
        /// </summary>
        /// <returns>a list of installed services</returns>
        public static List<ServiceDescription> GetServices()
        {
            var list = new List<ServiceDescription>();

            using (var mc = new ManagementClass("Win32_Service"))
            {
                foreach (var mo in mc.GetInstances())
                    using (mo)
                    {
                        list.Add(new ServiceDescription((string)mo.GetPropertyValue("Name"), (string)mo.GetPropertyValue("DisplayName")));
                    }
            }

            return list;
        }

        /// <summary>
        /// Starts the specified service.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="restartIfRunning">if set to <c>true</c> service should be restarted if already running.</param>
        public static void Start(string serviceName, bool restartIfRunning = false, int restartDelaySeconds = 30)
        {
            var service = new ServiceController(serviceName);
            if (service.Status == ServiceControllerStatus.Running)
            {
                if (!restartIfRunning) return;
                service.Stop();


                for (var i = 0; i < restartDelaySeconds && service.Status != ServiceControllerStatus.Stopped; i++)
                {
                    Thread.Sleep(1000);
                    service.Refresh();
                }
            }
            
            service.Start();
        }

        /// <summary>
        /// Stops the specified service.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        public static void Stop(string serviceName)
        {
            var service = new ServiceController(serviceName);
            if (service.Status == ServiceControllerStatus.Running) service.Stop();
        }
    }

    public class ServiceDescription
    {
        public string Name { get; private set; }
        public string DisplayName { get; private set; }

        public ServiceDescription(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }
    }
}
