using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.HAP.CharacteristicPocos
{
    public class ButtonEvent
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.One)]
        public int ButtonID { get; set; }

        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.One)]
        public int ButtonState { get; set; }

        [TLVCharacteristic(Type = 3, Length = TLVCharacteristicLength.Eight)]
        public ulong Timestamp { get; set; }

        [TLVCharacteristic(Type = 4, Length = TLVCharacteristicLength.Four)]
        public int ActiveIdentifier { get; set; }
    }
}
