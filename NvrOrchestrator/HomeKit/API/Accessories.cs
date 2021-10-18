using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NvrOrchestrator
{
    public static class Accessories
    {
        public static void GetAccessories(HomeKit.SessionVariables.Encryption enc, NetworkStream stream)
        {
            Console.WriteLine("GET /Accessories");            
            HomeKit.HTTP.HTTP.SendResponse(
                stream,
                Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(
                        HomeKit.HAP.HAPManager.SerializeFromPoco(Configuration.Global.Poco),
                        new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull })),
                "application/hap+json",
                enc);
        }

    }
}
