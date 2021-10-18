using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.HAP
{
    public class SupportedRTPConfiguration
    {
        [TLVCharacteristic(Type = 2, Length = TLVCharacteristicLength.One)]
        public SupportedRTPConfigurationSRTPCryptoSuite SRTPCryptoSuite { get; set; } = SupportedRTPConfigurationSRTPCryptoSuite.AES_CM_128_HMAC_SHA1_80;
    }

    public enum SupportedRTPConfigurationSRTPCryptoSuite
    {
        AES_CM_128_HMAC_SHA1_80 = 0,
        AES_256_CM_HMAC_SHA1_80 = 1,
        Disabled = 2
    }
}
