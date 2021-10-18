using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.HAP.Camera
{
    public class Poco
    {
        //8.1 Accessory Information
        [Service(UUID = GUIDs.public_hap_service_accessory_information)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_firmware_revision, Permissions = CharacteristicPermissions.PairedRead)]
        public string FirmwareRevision { get; private set; } = "1.2.0";

        //[Service(UUID = GUIDs.public_hap_service_accessory_information)]
        //[Characteristic(UUID = GUIDs.public_hap_characteristic_identify, Permissions = CharacteristicPermissions.PairedWrite)]
        //public bool Identify { private get; set; }

        [Service(UUID = GUIDs.public_hap_service_accessory_information)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_manufacturer, Permissions = CharacteristicPermissions.PairedRead)]
        public string Manufacturer { get; private set; } = "Phrodide";

        [Service(UUID = GUIDs.public_hap_service_accessory_information)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_model, Permissions = CharacteristicPermissions.PairedRead)]
        public string Model { get; private set; } = "SeeSharp NVR Orchestrator";

        [Service(UUID = GUIDs.public_hap_service_accessory_information)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_name, Permissions = CharacteristicPermissions.PairedRead)]
        public string Name { get; set; } = "SeeSharpCamera";

        [Service(UUID = GUIDs.public_hap_service_accessory_information)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_serial_number, Permissions = CharacteristicPermissions.PairedRead)]
        public string SerialNumber { get; private set; } = "2021.10.16";
        //8.17 HAP Protocol
        //8.6 Camera RTP Stream Management
        //8.27 Microphone
        //8.36 Speaker
        //8.12 Doorbell
        //If this is just an IP camera, expose 8.6, 8.27, and maybe 8.36
        //If this is a Camera Doorbell, expose 8.12 as primary, then the same as an IP Camera as secondary.

        //8.17 HAP Protocol (A2)
        //..... 9.125 Version (37)
        [Service(UUID = GUIDs.public_hap_service_protocol_information_service)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_version, Permissions = CharacteristicPermissions.PairedRead)]
        public string HapProtocolVersion { get; private set; } = "1.1.0";

        [Service(UUID = GUIDs.public_hap_service_camera_rtp_stream_management)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_active, Permissions = CharacteristicPermissions.PairedRead | CharacteristicPermissions.PairedWrite | CharacteristicPermissions.Events)]
        public byte Active { get; set; } = 1;
        //8.6 Characteristics: (110)
        //9.101 StreamingStatus (TLV that exposes:
        [Service(UUID = GUIDs.public_hap_service_camera_rtp_stream_management)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_stream_status, Permissions = CharacteristicPermissions.PairedRead | CharacteristicPermissions.Events)]
        public StreamingStatus StreamStatus { get; set; } = new();
        //9.102 SelectedRTPStreamConfiguration
        [Service(UUID = GUIDs.public_hap_service_camera_rtp_stream_management)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_selected_rtp_stream_configuration, Permissions = CharacteristicPermissions.PairedRead | CharacteristicPermissions.PairedWrite)]
        public SelectedRTPStreamConfiguration SelectedRTPStreamConfiguration { get; set; } = new();
        //9.92 Setup Endpoints (TLV that exposes:
        [Service(UUID = GUIDs.public_hap_service_camera_rtp_stream_management)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_setup_endpoints, Permissions = CharacteristicPermissions.PairedRead | CharacteristicPermissions.PairedWrite)]
        public SetupEndpoints SetupEndpoints { get; set; } = new();
        //9.102 Supported Audio Stream Configuration (TLV that exposes:
        [Service(UUID = GUIDs.public_hap_service_camera_rtp_stream_management)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_supported_audio_configuration, Permissions = CharacteristicPermissions.PairedRead)]
        public SupportedAudioStreamConfiguration SupportedAudioStreamConfiguration { get; set; } = new();

        //9.104 Supported RTP Configuration (TLV that exposes:
        [Service(UUID = GUIDs.public_hap_service_camera_rtp_stream_management)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_supported_rtp_configuration, Permissions = CharacteristicPermissions.PairedRead)]
        public SupportedRTPConfiguration SupportedRTPConfiguration { get; set; } = new();

        //9.105 Supported Video Stream Configuration (TLV that exposes: 
        [Service(UUID = GUIDs.public_hap_service_camera_rtp_stream_management)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_supported_video_configuration, Permissions = CharacteristicPermissions.PairedRead)]
        public SupportedVideoStreamConfiguration SupportedVideoConfiguration { get; set; } = new();

        //8.27 Characteristics: (112)
        //9.61 Mute (bool)
        [Service(UUID = GUIDs.public_hap_service_microphone)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_mute, Permissions = CharacteristicPermissions.PairedRead | CharacteristicPermissions.PairedWrite | CharacteristicPermissions.Events)]
        public bool MicrophoneMute { get; set; }
        //9.62 Name (string)
        [Service(UUID = GUIDs.public_hap_service_microphone)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_name, Permissions = CharacteristicPermissions.PairedRead | CharacteristicPermissions.PairedWrite | CharacteristicPermissions.Events)]
        public string MicrophoneName { get; set; } = "Microphone";
        //9.127 Volume (uint8, 0 to 100, step 1 (percentage))
        [Service(UUID = GUIDs.public_hap_service_microphone)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_volume, Permissions = CharacteristicPermissions.PairedRead | CharacteristicPermissions.PairedWrite | CharacteristicPermissions.Events)]
        public byte MicrophoneVolume { get; set; }

        //8.36 Characteristics: (113)
        //9.61 Mute (bool)
        //9.62 Name (string)
        //9.127 Volume (uint8, 0 to 100, step 1 (percentage))
        [Service(UUID = GUIDs.public_hap_service_speaker)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_mute, Permissions = CharacteristicPermissions.PairedRead | CharacteristicPermissions.PairedWrite | CharacteristicPermissions.Events)]
        public bool SpeakerMute { get; set; }
        //9.62 Name (string)
        [Service(UUID = GUIDs.public_hap_service_speaker)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_name, Permissions = CharacteristicPermissions.PairedRead | CharacteristicPermissions.PairedWrite | CharacteristicPermissions.Events)]
        public string SpeakerName { get; set; } = "Speaker";
        //9.127 Volume (uint8, 0 to 100, step 1 (percentage))
        [Service(UUID = GUIDs.public_hap_service_speaker)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_volume, Permissions = CharacteristicPermissions.PairedRead | CharacteristicPermissions.PairedWrite | CharacteristicPermissions.Events)]
        public byte SpeakerVolume { get; set; }

        //8.12 Doorbell Characteristics: (121)
        //9.76 Programmable Switch Event (report only in EVENT, reads must be null)
        [Service(UUID = GUIDs.public_hap_service_doorbell)]
        [Characteristic(UUID = GUIDs.public_hap_characteristic_input_event, Permissions = CharacteristicPermissions.PairedRead | CharacteristicPermissions.Events)]
        public byte? DoorbellButton { get; set; } = null;
        //9.62 Name (string)
        //9.127 Volume (uint8, 0 to 100, step 1 (precentage))
        //9.11 Brightness (int, 0 to 100, step 1 (precentage))
    }
}
