using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.HAP
{
    public class SupportedVideoStreamConfiguration
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.N)]
        public IEnumerable<VideoCodecConfiguration> VideoCodecConfiguration { get; set; } = new List<VideoCodecConfiguration>();
    }

    public class VideoCodecConfiguration
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.One)]
        public VideoCodecConfigurationVideoCodecType VideoCodecType { get; set; } = VideoCodecConfigurationVideoCodecType.H264;
        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.N)]
        public VideoCodecParameters VideoCodecParameters { get; set; } = new();
        [TLVCharacteristic(Type = 3, Length = TLVCharacteristicLength.N)]
        public IEnumerable<VideoAttributes> VideoAttributes { get; set; } = new List<VideoAttributes>();
    }

    public enum VideoCodecConfigurationVideoCodecType
    {
        H264 = 0
    }

    public class VideoCodecParameters
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.One)]
        public IEnumerable<int> ProfileID { get; set; } = new int[0];
        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.One)]
        public IEnumerable<int> Level { get; set; } = new int[0];
        [TLVCharacteristic(Type = 3, Length = TLVCharacteristicLength.One)]
        public IEnumerable<int> PacketizationMode { get; set; } = new int[0];
        //[TLVCharacteristic(Type = 4, Length = TLVCharacteristicLength.One)]
        //public int CVOEnabled { get; set; }
        //[TLVCharacteristic(Type = 5, Length = TLVCharacteristicLength.One)]
        //public int CVOID { get; set; }
    }

    public class VideoAttributes
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.Two)]
        public int ImageWidth { get; set; }
        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.Two)]
        public int ImageHeight { get; set; }
        [TLVCharacteristic(Type = 3, Length = TLVCharacteristicLength.One)]
        public int FrameRate { get; set; }
    }
}
