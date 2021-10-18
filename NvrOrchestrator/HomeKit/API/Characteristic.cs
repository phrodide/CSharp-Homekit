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
    public static class Characteristic
    {
        private static void GET(HomeKit.SessionVariables.Encryption enc, NetworkStream stream, HomeKit.HTTP.HTTPRequest request)
        {
            Console.WriteLine($"GET /Characteristics {request.Parameters} called, results {JsonSerializer.Serialize(HomeKit.HAP.HAPManager.SerializeFromParameterList(request.Parameters, Configuration.Global.Poco), new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull })}");
            HomeKit.HTTP.HTTP.SendResponse(
                stream,
                Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(
                        HomeKit.HAP.HAPManager.SerializeFromParameterList(request.Parameters, Configuration.Global.Poco),
                        new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull })),
                "application/hap+json",
                enc);
        }
        public static void PUT(List<HomeKit.Events.EventPoco> subscribedEvents, HomeKit.SessionVariables.Encryption enc, NetworkStream stream, HomeKit.HTTP.HTTPRequest request)
        {
            var putCommand = JsonSerializer.Deserialize<HomeKit.HAP.PutCharacteristicsContainer>(Encoding.UTF8.GetString(request.Body));
            foreach (var item in putCommand.characteristics.Where(pc => pc.ev == true))
            {
                Console.WriteLine($"Add Watch:{item.iid}");
                var ev = HomeKit.Events.EventManager.Subscribe(item.aid, item.iid, 0);
                ev.LastState = HomeKit.HAP.HAPManager.GetValueSingleCharacteristic(item.aid, item.iid, Configuration.Global.Poco);
                subscribedEvents.Add(ev);
            }
            foreach (var item in putCommand.characteristics.Where(pc => pc.ev == false))
            {
                foreach (var item2 in subscribedEvents.Where(l => l.aid == item.aid && l.iid == item.iid).ToArray())//This really should be one, but want to be sure.
                {
                    subscribedEvents.Remove(item2);
                    Console.WriteLine($"Remove Watch:{item.iid}");
                    HomeKit.Events.EventManager.Unsubscribe(item2);
                }
            }
            foreach (var item in putCommand.characteristics.Where(pc => pc.value != null))
            {
                Console.WriteLine($"PUT /Characteristics {item.aid}.{item.iid}={item.value}");
            }
            HomeKit.HAP.HAPManager.WriteChanges(Configuration.Global.Poco, putCommand);
            HomeKit.HTTP.HTTP.SendResponse(stream, null, enc: enc);//No Content Response
        }

        public static void Parser(List<HomeKit.Events.EventPoco> subscribedEvents, HomeKit.SessionVariables.Encryption enc, NetworkStream stream, HomeKit.HTTP.HTTPRequest request)
        {
            switch (request.Method)
            {
                case "GET":
                    GET(enc, stream, request);
                    break;
                case "PUT":
                    PUT(subscribedEvents, enc, stream, request);
                    break;
            }
        }

    }
}
