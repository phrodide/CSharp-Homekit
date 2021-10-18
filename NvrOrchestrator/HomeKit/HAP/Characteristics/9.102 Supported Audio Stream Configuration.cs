using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.HAP
{
    public class SupportedAudioStreamConfiguration
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.N)]
        public AudioCodecs AudioCodecConfiguration { get; set; } = new();
        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.One)]
        public int ComfortNoiseSupport { get; set; } = 0;
    }

    public class AudioCodecs
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.One)]
        public AudioCodecsCodecType CodecType { get; set; } = AudioCodecsCodecType.AAC_ELD;
        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.N)]
        public AudioCodecParameters AudioCodecParameters { get; set; } = new();

    }

    public enum AudioCodecsCodecType
    {
        AAC_ELD = 2,
        Opus = 3,
        AMR = 5,
        AMR_WB = 6
    }

    public class AudioCodecParameters
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.One)]
        public int AudioChannels { get; set; } = 1;
        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.One)]
        public AudioCodecParametersBitrate Bitrate { get; set; } = AudioCodecParametersBitrate.Variable;
        [TLVCharacteristic(Type = 3, Length = TLVCharacteristicLength.One)]
        public AudioCodecParametersSampleRate SampleRate { get; set; } = AudioCodecParametersSampleRate._16KHz;
        [TLVCharacteristic(Type = 4, Length = TLVCharacteristicLength.One, IsReadable = false)]
        public int RTPTime { get; set; }
    }

    public enum AudioCodecParametersBitrate
    {
        Variable = 0,
        Constant = 1
    }

    public enum AudioCodecParametersSampleRate
    {
        _8KHz = 0,
        _16KHz = 1,
        _24KHz = 2
    }
}
