using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace NvrOrchestrator.Bonjour
{
    public static class BonjourManager
    {
        public static BonjourPoco ConfigureBonjour(string UserVisibleName, string DeviceID, ushort Port, int CurrentConfigurationNum, PairingFeatureFlags PairingFeatureFlags, BonjourStatusFlags StatusFlags, AccessoryCategory AccessorCategory)
        {
            BonjourPoco p = new()
            {
                ActiveListeners = NetworkingHelpers.IPHelper.ActiveListeners(),
                ARecord = $"{UserVisibleName}._hap._tcp.local",
                PTRRecord = "_hap._tcp.local",
                Port = Port,
                TXTRecords = new string[]
                {
                    "txtvers=1",
                    $"c#={CurrentConfigurationNum}",
                    $"ff={(int)PairingFeatureFlags}",//
                    $"id={DeviceID}",
                    $"md={UserVisibleName}",
                    $"pv=1.1",
                    $"s#=1",
                    $"sf={(int)StatusFlags}",
                    $"ci={(int)AccessorCategory}"
                }
            };
            /* "txtvers=1",
                    "c#=1",
                    "ci=17",
                    "ff=0",
                    "id=24:4B:FE:E2:1D:E9",
                    "md=SeeSharpCamera",
                    "pv=1.1",
                    "s#=1",
                    "sf=0"*/
            return p;
        }
        public static byte[] EncodePacket(IEnumerable<mDNSResourcePoco> resources)
        {
            System.IO.MemoryStream mo = new();
            System.IO.BinaryWriter bw = new(mo);
            bw.WriteU16BE(0);//DNS ID, 0
            bw.WriteU16BE(0x8400);//Response, Authoritative
            bw.WriteU16BE((ushort)resources.Count(r => r.Category == mDNSResourceCategory.Question));
            bw.WriteU16BE((ushort)resources.Count(r => r.Category == mDNSResourceCategory.Answer));
            bw.WriteU16BE((ushort)resources.Count(r => r.Category == mDNSResourceCategory.NameServer));
            bw.WriteU16BE((ushort)resources.Count(r => r.Category == mDNSResourceCategory.AdditionalInfo));
            foreach (var q in resources.Where(r => r.Category== mDNSResourceCategory.Question))
            {
                bw.Write(q.ToByteArray(mo.ToArray()));
            }
            foreach (var q in resources.Where(r => r.Category == mDNSResourceCategory.Answer))
            {
                bw.Write(q.ToByteArray(mo.ToArray()));
            }
            foreach (var q in resources.Where(r => r.Category == mDNSResourceCategory.NameServer))
            {
                bw.Write(q.ToByteArray(mo.ToArray()));
            }
            foreach (var q in resources.Where(r => r.Category == mDNSResourceCategory.AdditionalInfo))
            {
                bw.Write(q.ToByteArray(mo.ToArray()));
            }


            bw.Flush();
            return mo.ToArray();
        }
        public static List<mDNSResourcePoco> DecodePacket(byte[] packet)
        {
            System.IO.MemoryStream mi = new(packet);
            System.IO.BinaryReader bi = new(mi);
            var ID = bi.ReadU16BE();
            var Flags = bi.ReadU16BE();
            var QCount = bi.ReadU16BE();
            var ACount = bi.ReadU16BE();
            var NCount = bi.ReadU16BE();
            var AdCount = bi.ReadU16BE();
            List<mDNSResourcePoco> tempList = new();
            for (int q = 0; q < QCount; q++)
            {
                tempList.Add(DecodeSingleResource(mDNSResourceCategory.Question,bi,packet));
            }
            for (int a = 0; a < ACount; a++)
            {
                tempList.Add(DecodeSingleResource(mDNSResourceCategory.Answer, bi, packet));
            }
            for (int n = 0; n < NCount; n++)
            {
                tempList.Add(DecodeSingleResource(mDNSResourceCategory.NameServer, bi, packet));
            }
            for (int ad = 0; ad < AdCount; ad++)
            {
                tempList.Add(DecodeSingleResource(mDNSResourceCategory.AdditionalInfo, bi, packet));
            }
            return tempList;
        }
        private static mDNSResourcePoco DecodeSingleResource(mDNSResourceCategory category, System.IO.BinaryReader bi, byte[] packet)
        {
            var r = new mDNSResourcePoco()
            {
                Category = category,
                Name = bi.DecodeName(packet),
                ResourceType = (mDNSResourceTypes)bi.ReadU16BE(),
                ResourceClass = (mDNSResourceClass)bi.ReadU16BE(),
                TTL = category == mDNSResourceCategory.Question ? 0 : bi.ReadU32BE(),
                ResourceData = category == mDNSResourceCategory.Question ? null : bi.ReadBytes(bi.ReadU16BE())
            };
            if (category != mDNSResourceCategory.Question)
            {
                System.IO.BinaryReader bid = new(new System.IO.MemoryStream(r.ResourceData));
                switch (r.ResourceType)
                {
                    case mDNSResourceTypes.SRV:
                        r.SRV_Priority = bid.ReadU16BE();
                        r.SRV_Weight = bid.ReadU16BE();
                        r.SRV_Port = bid.ReadU16BE();
                        r.SRV_Target = bid.DecodeName(packet);
                        break;
                    case mDNSResourceTypes.PTR:
                        r.PTR_DomainNamePointer = bid.DecodeName(packet);
                        break;
                    case mDNSResourceTypes.AAAA:
                        r.AAAA_IPAddress = new(r.ResourceData);
                        break;
                    case mDNSResourceTypes.A:
                        r.A_IPAddress = new(r.ResourceData);
                        break;
                    case mDNSResourceTypes.TXT:
                        while (true)
                        {
                            if (bid.BaseStream.Position == bid.BaseStream.Length)
                                break;
                            int len = bid.ReadByte();
                            if (len == 0)
                                break;
                            bid.BaseStream.Position -= 1;
                            r.TXT.Add(bid.ReadString());
                        }
                        break;
                    case mDNSResourceTypes.NSEC:
                        r.NSEC_NXTDomainName = bid.DecodeName(packet);
                        //bid.BaseStream.Position = bid.BaseStream.Length - 4;
                        r.NSEC_Error = 0;
                        break;
                    case mDNSResourceTypes.OPT://What is the purpose of this?????
                        break;
                    case mDNSResourceTypes.NS:
                        r.NS_NameServer = bid.DecodeName(packet);
                        break;
                    default:
                        Console.WriteLine("Unknown DNS Record Type:" + r.ResourceType);
                        break;
                }
            }
            return r;
        }

    }
}
