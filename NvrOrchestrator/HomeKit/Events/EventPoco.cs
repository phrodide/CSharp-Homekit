using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.Events
{
    public class EventPoco
    {
        private volatile bool _triggered = false;
        public int aid { get; set; }
        public int iid { get; set; }
        public int SubscribingID { get; set; }//The class should input a reference


        public bool Triggered { 
            get
            {
                return _triggered;
            }
            set
            {
                _triggered = value;
            }
        }

        public object TriggerValue { get; set; }

        public object LastState { get; set; }
    }
}
