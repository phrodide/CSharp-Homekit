using NvrOrchestrator.Encryption.Curve25519;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.HomeKit.SessionVariables
{
    public class Encryption
    {
        public Encryption(Keys keys)
        {
            Keys = keys;
            AccessoryToControllerKey = System.Security.Cryptography.HKDF.DeriveKey(
                    System.Security.Cryptography.HashAlgorithmName.SHA512,
                    keys.sharedSecret,
                    32,
                    Encoding.UTF8.GetBytes("Control-Salt"),
                    Encoding.UTF8.GetBytes("Control-Read-Encryption-Key"));
            ControllerToAccessoryKey = System.Security.Cryptography.HKDF.DeriveKey(
                System.Security.Cryptography.HashAlgorithmName.SHA512,
                keys.sharedSecret,
                32,
                Encoding.UTF8.GetBytes("Control-Salt"),
                Encoding.UTF8.GetBytes("Control-Write-Encryption-Key"));
        }
        public Keys Keys { get; private set; }
        public byte[] AccessoryToControllerKey { get; private set; }
        public byte[] ControllerToAccessoryKey { get; private set; }
        public long ReadNonce { get; set; }
        public long WriteNonce { get; set; }
        public bool IsEncrypted { get; set; } = false;
    }
}
