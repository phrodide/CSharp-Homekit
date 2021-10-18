using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.NetworkingHelpers
{
    public static class IPHelper
    {
        public static IEnumerable<System.Net.IPAddress> ActiveListeners() => System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                                        .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                                        .Where(ip => ip.IsDnsEligible)
                                        .Select(ip => ip.Address);
    }
}
