using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.HAP
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class TLVCharacteristicAttribute : System.Attribute
    {
        public int Type { get; set; }
        public TLVCharacteristicLength Length { get; set; }
        public bool IsWritable { get; set; } = true;
        public bool IsReadable { get; set; } = true;
    }

    public enum TLVCharacteristicLength//realistically this is used on writes, as reads already have this.
    {
        One = 1,
        Two = 2,
        Four = 4,
        Eight = 8,
        GUID = 16,
        N = 255
    }
}
