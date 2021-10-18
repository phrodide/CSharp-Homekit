using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.Events
{
    public static class EventManager
    {
        public static (int aid, int iid) GetLocationOfCharacteristic(Guid service, Guid characteristic)
        {
            foreach (var acc in HAP.HAPManager.SerializeFromPoco(Configuration.Global.Poco).accessories)
            {
                var item = acc.services.Where(s => s.type == service).FirstOrDefault().characteristics.Where(c => c.type == characteristic).FirstOrDefault();
                return (acc.aid, item.iid);
            }
            return (0, 0);
        }
        public static void Trigger(int aid, int iid, object TriggerValue)
        {
            foreach (var item in Configuration.Global.EventSubscriptions.Where(e => e.aid == aid && e.iid == iid))
            {
                item.Triggered = true;
                item.TriggerValue = TriggerValue;
            }
        }

        public static EventPoco Subscribe(int aid, int iid, int SubscribingID)
        {
            EventPoco retVal = new() { aid = aid, iid = iid, SubscribingID = SubscribingID, Triggered = false };
            Configuration.Global.EventSubscriptions.Add(retVal);
            return retVal;
        }

        public static void Unsubscribe(EventPoco ev)
        {
            Configuration.Global.EventSubscriptions.Remove(ev);
        }
    }
}
