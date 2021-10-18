using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.HAP
{
    public class SelectedRTPStreamConfiguration
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.N)]
        public SessionControlCommand SessionControl { get; set; } = new();

        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.N)]
        public SelectedVideoParameters SelectedVideoParameters { get; set; } = new();

        [TLVCharacteristic(Type = 3, Length = TLVCharacteristicLength.N)]
        public SelectedAudioParameters SelectedAudioParameters { get; set; } = new();
    }

    public class SessionControlCommand
    {

        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.GUID)]
        public Guid SessionIdentifier { get; set; }
        
        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.One)]
        public int Command
        {
            get => 1;
            set
            {
                if (value==1)
                {
                    _ = Task.Run(Camera.SampleFFMpegStream.FFMPEG);
                } 
                else if (value==0)
                {
                    Configuration.Global.ActiveCamera = 0;
                }
            }
        }
    }

    public class SelectedVideoParameters
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.One)]
        public int SelectedVideoCodecType { get; set; }

        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.N)]
        public VideoCodecParameters SelectedVideoCodecParameters { get; set; } = new();

        [TLVCharacteristic(Type = 3, Length = TLVCharacteristicLength.N)]
        public VideoAttributes SelectedVideoAttributes { get; set; } = new();

        [TLVCharacteristic(Type = 4, Length = TLVCharacteristicLength.N)]
        public VideoRTPParameters SelectedVideoRTPParameters { get; set; } = new();
    }

    public class VideoRTPParameters
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.One)]
        public int PayloadType { get; set; }

        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.Four)]
        public int SyncSource { get; set; }

        [TLVCharacteristic(Type = 3, Length = TLVCharacteristicLength.Two)]
        public int MaxBitrate { get; set; }

        [TLVCharacteristic(Type = 4, Length = TLVCharacteristicLength.Four)]
        public int MinRTCPInterval { get; set; }

        [TLVCharacteristic(Type = 5, Length = TLVCharacteristicLength.Two)]
        public int MaxMTU { get; set; }
    }
    public class SelectedAudioParameters
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.One)]
        public int SelectedAudioCodecType { get; set; }

        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.N)]
        public AudioCodecParameters SelectedAudioCodecParameters { get; set; } = new();

        [TLVCharacteristic(Type = 3, Length = TLVCharacteristicLength.N)]
        public AudioRTPParameters SelectedAudioRTPParameters { get; set; } = new();

        [TLVCharacteristic(Type = 4, Length = TLVCharacteristicLength.One)]
        public int ComfortNoise { get; set; }
    }

    public class AudioRTPParameters
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.One)]
        public int PayloadType { get; set; }

        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.Four)]
        public int SyncSource { get; set; }

        [TLVCharacteristic(Type = 3, Length = TLVCharacteristicLength.Two)]
        public int MaxBitrate { get; set; }

        [TLVCharacteristic(Type = 4, Length = TLVCharacteristicLength.Four)]
        public int MinRTCPInterval { get; set; }

        [TLVCharacteristic(Type = 6, Length = TLVCharacteristicLength.One)]
        public int ComfortNoisePayloadType { get; set; }
    }
}
