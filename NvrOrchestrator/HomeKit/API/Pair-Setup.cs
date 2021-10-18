using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using NvrOrchestrator.HomeKit.TLV;
using System.Collections.Generic;
using NvrOrchestrator.Encryption.ChaCha20Poly1305;

namespace NvrOrchestrator
{
    public static class Pair_Setup
    {
        public static void PairSetup(NetworkStream stream, IEnumerable<TLV> tlvs)
        {
            var state = (from t in tlvs where t.Type == HomeKit.TLV.TLV_Type.kTLVType_State select t.Value).Single();
            if (state[0] == 1)
            {
                //M1
                //we got an M1 message. We need to generate a public key and send it back.
                TLV[] a = new[] {
                                                    new TLV(TLV_Type.kTLVType_State, TLVConstants.M2),
                                                    new TLV(TLV_Type.kTLVType_PublicKey, Program.srp.B.ToByteArray()),
                                                    new TLV(TLV_Type.kTLVType_Salt, Program.srp.s.ToByteArray())
                                                };
                HomeKit.HTTP.HTTP.SendResponse(stream, TLVManager.Encode(a));
            }
            else if (state[0] == 3)
            {
                //M3
                var msB = new MemoryStream();
                foreach (var tlv in tlvs.Where(x => x.Type == TLV_Type.kTLVType_PublicKey))
                {
                    msB.Write(tlv.Value, 0, tlv.Value.Length);
                }
                Program.srp.A = new(msB.ToArray());
                var M1 = new Encryption.SRP.SrpBigInteger(tlvs.Where(x => x.Type == TLV_Type.kTLVType_Proof).Single().Value);
                if (M1 == Program.srp.M1)
                {
                    TLV[] m3 = new[] {
                                                        new TLV(TLV_Type.kTLVType_State, TLVConstants.M4),
                                                        new TLV(TLV_Type.kTLVType_Proof, Program.srp.M2.ToByteArray())
                                                    };
                    HomeKit.HTTP.HTTP.SendResponse(stream, TLVManager.Encode(m3));
                }
                else
                {
                    TLV[] m3 = new[] {
                                                        new TLV(TLV_Type.kTLVType_State, TLVConstants.M4),
                                                        new TLV(TLV_Type.kTLVType_Error, TLVConstants.kTLVError_Authentication)
                                                    };
                    HomeKit.HTTP.HTTP.SendResponse(stream, TLVManager.Encode(m3));
                }
            }
            else if (state[0] == 5)
            {
                //M5
                var iOSDeviceX = System.Security.Cryptography.HKDF.DeriveKey(
                    System.Security.Cryptography.HashAlgorithmName.SHA512,
                    Program.srp.K2.ToByteArray(),
                    32,
                    Encoding.UTF8.GetBytes("Pair-Setup-Encrypt-Salt"),
                    Encoding.UTF8.GetBytes("Pair-Setup-Encrypt-Info"));
                var encryptedData = tlvs.Where(x => x.Type == TLV_Type.kTLVType_EncryptedData).Single().Value;
                var chacha20Data = encryptedData.Take(encryptedData.Length - 16).ToArray();
                //var plaintextData = new byte[chacha20Data.Length];
                var authData = encryptedData.TakeLast(16).ToArray();
                //var decryptor = new NaCl.Core.ChaCha20Poly1305(iOSDeviceX);
                //decryptor.Decrypt(Encoding.UTF8.GetBytes("\0\0\0\0PS-Msg05"), chacha20Data, authData, plaintextData);
                var plaintextData = ChaChaPolyManager.ChaCha20Poly1305_Decrypt(Encoding.UTF8.GetBytes("\0\0\0\0PS-Msg05"), chacha20Data, null, iOSDeviceX, authData);
                var subTlv = TLVManager.Decode(plaintextData);

                byte[] DeviceLTPK = subTlv.Where(x => x.Type == TLV_Type.kTLVType_PublicKey).Single().Value;
                byte[] DevicePairingID = subTlv.Where(x => x.Type == TLV_Type.kTLVType_Identifier).Single().Value;


                var iOSDeviceX_part2 = System.Security.Cryptography.HKDF.DeriveKey(
                    System.Security.Cryptography.HashAlgorithmName.SHA512,
                    Program.srp.K2.ToByteArray(),
                    32,
                    Encoding.UTF8.GetBytes("Pair-Setup-Controller-Sign-Salt"),
                    Encoding.UTF8.GetBytes("Pair-Setup-Controller-Sign-Info"));
                var signatureData = iOSDeviceX_part2.Concat(DevicePairingID).Concat(DeviceLTPK).ToArray();
                var signature = subTlv.Where(x => x.Type == TLV_Type.kTLVType_Signature).Single().Value;
                var result = Chaos.NaCl.Ed25519.Verify(signature, signatureData, DeviceLTPK);
                if (result)
                {
                    File.WriteAllBytes($"{Configuration.Global.Poco.Name}/Devices/{Encoding.UTF8.GetString(DevicePairingID)}.bin", DeviceLTPK);

                    var AccessoryX = System.Security.Cryptography.HKDF.DeriveKey(
                        System.Security.Cryptography.HashAlgorithmName.SHA512,
                        Program.srp.K2.ToByteArray(),
                        32,
                        Encoding.UTF8.GetBytes("Pair-Setup-Accessory-Sign-Salt"),
                        Encoding.UTF8.GetBytes("Pair-Setup-Accessory-Sign-Info"));
                    var AccessoryInfo = AccessoryX.Concat(Encoding.UTF8.GetBytes(Configuration.Global.ID)).Concat(Configuration.Global.LTPK).ToArray();
                    var Signature = Chaos.NaCl.Ed25519.Sign(AccessoryInfo, Configuration.Global.LTSK);
                    TLV[] em6 = new[] {
                                                        new TLV(TLV_Type.kTLVType_Identifier, Encoding.UTF8.GetBytes(Configuration.Global.ID)),
                                                        new TLV(TLV_Type.kTLVType_PublicKey, Configuration.Global.LTPK),
                                                        new TLV(TLV_Type.kTLVType_Signature, Signature),
                                                    };
                    var plainText = TLVManager.Encode(em6);
                    var crypText = ChaCha20Poly1305.ChaChaEncryptHelper(iOSDeviceX, plainText, "PS-Msg06");

                    TLV[] m6 = new[] {
                                                        new TLV(TLV_Type.kTLVType_State, TLVConstants.M6),
                                                        new TLV(TLV_Type.kTLVType_EncryptedData, crypText)
                                                    };
                    HomeKit.HTTP.HTTP.SendResponse(stream, TLVManager.Encode(m6));

                }
            }
        }
    }
}
