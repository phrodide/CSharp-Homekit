using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.HAP
{
    class CharacteristicAttribute : System.Attribute
    {
        public string UUID { get; set; }
        public CharacteristicPermissions Permissions { get; set; }
    }

    public enum CharacteristicPermissions
    {
        PairedRead = 1,
        PairedWrite = 2,
        Events = 4,
        AdditionalAuthorization = 8,
        TimedWrite = 16,
        Hidden = 32,
        WriteResponse = 64
    }
}
