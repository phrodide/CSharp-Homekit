using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.HAP
{
    class ServiceAttribute : System.Attribute
    {
        public string UUID { get; set; }
        public string Type { get; set; }
        public ServiceAttribute()
        {

        }
    }
}
