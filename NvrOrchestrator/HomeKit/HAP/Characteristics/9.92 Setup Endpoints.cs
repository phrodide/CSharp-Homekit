using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.HAP
{
    public class ControllerAddress
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.One)]
        public ControllerAddressIPAddressVersion IPAddressVersion { get; set; }

        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.N)]
        public string IPAddress { get; set; } = "";

        [TLVCharacteristic(Type = 3, Length = TLVCharacteristicLength.Two)]
        public int VideoRTPPort { get; set; }

        [TLVCharacteristic(Type = 4, Length = TLVCharacteristicLength.Two)]
        public int AudioRTPPort { get; set; }

    }

    public enum ControllerAddressIPAddressVersion
    {
        IPv4 = 0,
        IPv6 = 1
    }

    public class SRTPCryptoSuiteClass
    {
        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.One)]
        public SupportedRTPConfigurationSRTPCryptoSuite SRTPCryptoSuite { get; set; }

        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.N)]
        public byte[] SRTPCryptoKey { get; set; } = new byte[] { };

        [TLVCharacteristic(Type = 3, Length = TLVCharacteristicLength.N)]
        public byte[] SRTPMasterSalt { get; set; } = new byte[] { };
    }

    public class SetupEndpoints
    {

        [TLVCharacteristic(Type = 1, Length = TLVCharacteristicLength.GUID)]//read and write
        public Guid SessionID { get; set; }

        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.One, IsWritable = false)]//read only. This bumps the ordinal by one from the read / write, otherwise it is basically the same order.
        public SetupEndpointsStatus Status { get; set; }
        [TLVCharacteristic(Type = 3, Length = TLVCharacteristicLength.N, IsReadable = false)]//write only
        public ControllerAddress ControllerAddress { get; set; } = new();

        [TLVCharacteristic(Type = 3, Length = TLVCharacteristicLength.N, IsWritable = false)]//read only
        public ControllerAddress AccessoryAddress { get; set; } = new();

        [TLVCharacteristic(Type = 4, Length = TLVCharacteristicLength.N)]
        public SRTPCryptoSuiteClass SRTPParametersForVideo { get; set; } = new();
        [TLVCharacteristic(Type = 5, Length = TLVCharacteristicLength.N)]
        public SRTPCryptoSuiteClass SRTPParametersForAudio { get; set; } = new();
        [TLVCharacteristic(Type = 6, Length = TLVCharacteristicLength.Four, IsWritable = false)]
        public int VideoSyncSource { get; set; }
        [TLVCharacteristic(Type = 7, Length = TLVCharacteristicLength.Four, IsWritable = false)]
        public int AudioSyncSource { get; set; }
    }

    public enum SetupEndpointsStatus
    {
        Success=0,
        Busy=1,
        Error=2
    }
}
