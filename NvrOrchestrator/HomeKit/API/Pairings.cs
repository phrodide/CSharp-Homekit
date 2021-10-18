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

namespace NvrOrchestrator
{
    public static class Pairing
    {
        public static void Pairings(NetworkStream stream, IEnumerable<TLV> tlvs, HomeKit.SessionVariables.Encryption enc)
        {
            var state = (from t in tlvs where t.Type == HomeKit.TLV.TLV_Type.kTLVType_State select t.Value).Single();
            if (state[0] == 1)
            {
                var method = (from t in tlvs where t.Type == HomeKit.TLV.TLV_Type.kTLVType_Method select t.Value).Single();
                switch (method[0])
                {
                    case 3://add pairing
                        {
                            var AdditionalControllerPairingIdentifier = (from t in tlvs where t.Type == TLV_Type.kTLVType_Identifier select t.Value).Single();
                            var AdditionalControllerLTPK = (from t in tlvs where t.Type == TLV_Type.kTLVType_PublicKey select t.Value).Single();
                            var AdditionalControllerPermissions = (from t in tlvs where t.Type == TLV_Type.kTLVType_Permissions select t.Value).Single();
                            Console.WriteLine($"AddPairing {Encoding.UTF8.GetString(AdditionalControllerPairingIdentifier)}");
                            File.WriteAllBytes($"{Configuration.Global.Poco.Name}/Devices/{Encoding.UTF8.GetString(AdditionalControllerPairingIdentifier)}.bin", AdditionalControllerLTPK);

                            TLV[] m2 = new[] {
                                new TLV(TLV_Type.kTLVType_State, TLVConstants.M2)
                            };
                            HomeKit.HTTP.HTTP.SendResponse(stream, TLVManager.Encode(m2), enc: enc);
                        }
                        break;
                    case 4://delete pairing
                        {
                            var AdditionalControllerPairingIdentifier = (from t in tlvs where t.Type == TLV_Type.kTLVType_Identifier select t.Value).Single();

                            try
                            {
                                Console.WriteLine($"RemovePairing {Encoding.UTF8.GetString(AdditionalControllerPairingIdentifier)}");
                                File.Delete($"{Configuration.Global.Poco.Name}/Devices/{Encoding.UTF8.GetString(AdditionalControllerPairingIdentifier)}.bin");
                            }
                            catch (Exception)
                            {

                            }

                            TLV[] m2 = new[] {
                                new TLV(TLV_Type.kTLVType_State, TLVConstants.M2)
                            };
                            HomeKit.HTTP.HTTP.SendResponse(stream, TLVManager.Encode(m2), enc: enc);
                        }
                        break;
                    case 5://list pairings
                        {
                            Console.WriteLine($"ListPairings");
                            List<TLV> m2 = new();
                            m2.Add(new TLV(TLV_Type.kTLVType_State, TLVConstants.M2));
                            var files = Directory.GetFiles($"{Configuration.Global.Poco.Name}/Devices/");
                            foreach (var file in files)
                            {
                                var PublicKey = File.ReadAllBytes(file);
                                var name = file[(file.LastIndexOf("Devices") + 8)..].Replace(".bin", "");
                                if (m2.Count!=1)
                                {
                                    m2.Add(new TLV(TLV_Type.kTLVType_Separator, TLVConstants.NoValue));
                                }
                                m2.Add(new TLV(TLV_Type.kTLVType_Identifier, Encoding.UTF8.GetBytes(name)));
                                m2.Add(new TLV(TLV_Type.kTLVType_PublicKey, PublicKey));
                                m2.Add(new TLV(TLV_Type.kTLVType_Permissions, TLVConstants.Permissions_Admin));
                            }
                            HomeKit.HTTP.HTTP.SendResponse(stream, TLVManager.Encode(m2), enc: enc);
                        }
                        break;
                }
            }
        }
    }
}
