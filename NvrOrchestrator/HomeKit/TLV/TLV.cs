using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NvrOrchestrator.HomeKit.TLV
{
    public class TLVManager
    {
        public static IEnumerable<TLV> Decode(byte[] data)
        {
            List<TLV> tlv = new();
            MemoryStream ms = new(data);
            while (ms.Position != ms.Length)
            {
                var Type = (TLV_Type)ms.ReadByte();
                var Length = ms.ReadByte();
                byte[] tempStorage = new byte[Length];
                ms.Read(tempStorage, 0, Length);
                tlv.Add(new(Type, tempStorage));
            };
            return tlv;
        }
        public static byte[] Encode(IEnumerable<TLV> data)
        {
            
            MemoryStream ms = new MemoryStream();
            foreach (var item in data)
            {
                var tlvLength = item.Value.Length;
                for (int i = 0; i <= tlvLength/256; i++)
                {
                    int Length = tlvLength - (i * 255);
                    Length = Length >= 256 ? 255 : Length;
                    ms.WriteByte((byte)item.Type);
                    ms.WriteByte((byte)Length);
                    ms.Write(item.Value,i*255,Length);
                }
            }
            return ms.ToArray();            
        }
    }

    public record TLV (TLV_Type Type, byte[] Value);

    public enum TLV_Type
    {
        kTLVType_Method=0,
        kTLVType_Identifier=1,
        kTLVType_Salt=2,
        kTLVType_PublicKey=3,
        kTLVType_Proof=4,
        kTLVType_EncryptedData=5,
        kTLVType_State=6,
        kTLVType_Error=7,
        kTLVType_RetryDelay=8,
        kTLVType_Certificate=9,
        kTLVType_Signature=10,
        kTLVType_Permissions=11,
        kTLVType_FragmentData=12,
        kTLVType_FragmentLast=13,
        kTLVType_Flags=19,
        kTLVType_Separator=255


        /*
         * 0x00 kTLVType_Method integer Method to use for pairing. See Table 5-3 (page 49).
         * 0x01 kTLVType_Identifier UTF-8 Identifier for authentication.
         * 0x02 kTLVType_Salt bytes 16+ bytes of random salt.
         * 0x03 kTLVType_PublicKey bytes Curve25519, SRP public key, or signed Ed25519 key.
         * 0x04 kTLVType_Proof bytes Ed25519 or SRP proof.
         * 0x05 kTLVType_EncryptedData bytes Encrypted data with auth tag at end.
         * 0x06 kTLVType_State integer State of the pairing process. 1=M1, 2=M2, etc.
         * 0x07 kTLVType_Error integer Error code. Must only be present if error code is not 0. See Table 5-5 (page 50).
         * 0x08 kTLVType_RetryDelay integer Seconds to delay until retrying a setup code.
         * 0x09 kTLVType_Certificate bytes X.509 Certificate.
         * 0x0A kTLVType_Signature bytes Ed25519
         * 0x0B kTLVType_Permissions integer Bit value describing permissions of the controller being added.
         * * None (0x00) : Regular user
         * * Bit 1 (0x01) : Admin that is able to add and remove pairings against the accessory.
         * 0x0C kTLVType_FragmentData bytes Non-last fragment of data. If length is 0, itʼs an ACK.
         * 0x0D kTLVType_FragmentLast bytes Last fragment of data.
         * 0x13 kTLVType_Flags integer Pairing Type Flags (32 bit unsigned integer). See Table 5-7 (page 51)
         * 0xFF kTLVType_Separator null Zero-length TLV that separates different TLVs in a list
         */
    }

    public static class TLVConstants
    {
        public static byte[] M1 = new byte[] { 1 };
        public static byte[] M2 = new byte[] { 2 };
        public static byte[] M3 = new byte[] { 3 };
        public static byte[] M4 = new byte[] { 4 };
        public static byte[] M5 = new byte[] { 5 };
        public static byte[] M6 = new byte[] { 6 };

        public static byte[] kTLVError_Unknown = new byte[] { 1 };
        public static byte[] kTLVError_Authentication = new byte[] { 2 };
        public static byte[] kTLVError_Backoff = new byte[] { 3 };
        public static byte[] kTLVError_MaxPeers = new byte[] { 4 };
        public static byte[] kTLVError_MaxTries = new byte[] { 5 };
        public static byte[] kTLVError_Unavailable = new byte[] { 6 };
        public static byte[] kTLVError_Busy = new byte[] { 7 };

        public static byte[] Permissions_Standard = new byte[] { 0 };
        public static byte[] Permissions_Admin = new byte[] { 1 };

        public static byte[] NoValue = new byte[0];

    }
}
