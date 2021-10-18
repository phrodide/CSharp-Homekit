using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.HAP
{
    public class SelectedAudioConfiguration
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.N)]
        public IEnumerable<SelectedAudioInputStreamConfiguration> SelectedAudioInputStreamConfiguration { get; set; } = new List<SelectedAudioInputStreamConfiguration>();
    }


    public class SelectedAudioInputStreamConfiguration { 

        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.One)]
        public int SelectedAudioCodecType { get; set; }

        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.N)]
        public AudioCodecParameters SelectedAudioCodecParameters { get; set; } = new();
    }
}
