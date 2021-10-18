using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.HAP
{
    //This class gave me the wrong impression about Apple's use of TLVs. Then I found 102, 103, 104, and 105. The Matroshka dolls of the TLV
    public class StreamingStatus
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.One)]
        public StreamingStatusStatus Status { get; set; } = StreamingStatusStatus.Available;
    }

    public enum StreamingStatusStatus
    {
        Available = 0,
        InUse = 1,
        Unavailable = 2
    }
}
