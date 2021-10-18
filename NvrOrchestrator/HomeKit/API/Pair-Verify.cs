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
    public static class Pair_Verify
    {
        public static void PairVerify(NetworkStream stream, IEnumerable<TLV> tlvs, ref HomeKit.SessionVariables.Encryption enc)
        {
            var state = (from t in tlvs where t.Type == TLV_Type.kTLVType_State select t.Value).Single();
            if (state[0] == 1)
            {
                enc = new(new()
                {
                    ownPrivate = null,//force a random key to generate
                    remotePublic = (from t in tlvs where t.Type == TLV_Type.kTLVType_PublicKey select t.Value).Single()
                });

                var AccessoryInfo = enc.Keys.ownPublic.Concat(Encoding.UTF8.GetBytes(Configuration.Global.ID)).Concat(enc.Keys.remotePublic).ToArray();
                var Signature = Chaos.NaCl.Ed25519.Sign(AccessoryInfo, Configuration.Global.LTSK);
                TLV[] em2 = new[] {
                                                    new TLV(TLV_Type.kTLVType_Identifier, Encoding.UTF8.GetBytes(Configuration.Global.ID)),
                                                    new TLV(TLV_Type.kTLVType_Signature, Signature)
                                                };
                var plainText = TLVManager.Encode(em2);
                var crypText = ChaCha20Poly1305.ChaChaEncryptHelper(plainText, enc.Keys.sharedSecret, "PV-Msg02", "Pair-Verify-Encrypt-Salt", "Pair-Verify-Encrypt-Info");
                TLV[] m2 = new[] {
                                                    new TLV(TLV_Type.kTLVType_State, TLVConstants.M2),
                                                    new TLV(TLV_Type.kTLVType_PublicKey, enc.Keys.ownPublic),
                                                    new TLV(TLV_Type.kTLVType_EncryptedData, crypText)
                                                };
                HomeKit.HTTP.HTTP.SendResponse(stream, TLVManager.Encode(m2));

            }
            else if (state[0] == 3)
            {
                TLV[] m4 = new[] {
                    new TLV(TLV_Type.kTLVType_State, TLVConstants.M4)
                };
                HomeKit.HTTP.HTTP.SendResponse(stream, TLVManager.Encode(m4));
                enc.IsEncrypted = true;
            }
        }

    }
}
