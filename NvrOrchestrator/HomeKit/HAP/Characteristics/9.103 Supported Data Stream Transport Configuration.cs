using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.HAP
{
    class SupportedDataStreamTransportConfiguration
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.N)]
        public byte[] TransferTransportConfiguration { get; set; } = new byte[0];
    }
}
