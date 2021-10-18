using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.Bonjour
{
    public static class BonjourExtensions
    {
        public static void WriteU16BE(this System.IO.BinaryWriter bw, ushort data)
        {
            bw.Write((byte)((data & 0xFF00U) >> 8));
            bw.Write((byte)(data & 0xFFU));
        }
        public static void WriteU32BE(this System.IO.BinaryWriter bw, uint data)
        {
            bw.Write((byte)((data & 0xFF000000U) >> 24));
            bw.Write((byte)((data & 0xFF0000U) >> 16));
            bw.Write((byte)((data & 0xFF00U) >> 8));
            bw.Write((byte)(data & 0xFFU));
        }
        public static ushort ReadU16BE(this System.IO.BinaryReader bi)
        {
            ushort data = bi.ReadUInt16();
            return (ushort)((data & 0xFFU) << 8 | (data & 0xFF00U) >> 8);
        }
        public static uint ReadU32BE(this System.IO.BinaryReader bi)
        {
            uint data = bi.ReadUInt32();
            return (uint)(data & 0x000000FFU) << 24 | (data & 0x0000FF00U) << 8 | (data & 0x00FF0000U) >> 8 | (data & 0xFF000000U) >> 24;
        }
        public static IEnumerable<int> PatternAt(this byte[] source, byte[] pattern)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    yield return i;
                }
            }
        }

        public static string DecodeName(this System.IO.BinaryReader bi, byte[] packet)
        {
            System.Text.StringBuilder sb = new StringBuilder();
            while (true)
            {
                var len = bi.ReadByte();
                if (len>=0xC0)
                {
                    //This is a pointer.
                    ushort offset = (ushort)(((len - 0xC0) << 8) + bi.ReadByte());
                    var tempBi = new System.IO.BinaryReader(new System.IO.MemoryStream(packet));
                    tempBi.BaseStream.Position = offset;
                    if (sb.Length != 0) sb.Append('.');
                    sb.Append(DecodeName(tempBi, packet));
                    break;
                }
                if (len==0)
                {
                    break;
                }
                if (sb.Length != 0) sb.Append('.');
                bi.BaseStream.Position -= 1;
                sb.Append(bi.ReadString());
            }
            return sb.ToString();
        }

        private static byte[] LabelsToByte(string[] labels)
        {
            byte[] tester = new byte[labels.Select(l => l.Length+1).Sum()];
            int offset = 0;
            foreach (var item in labels)
            {
                tester[offset++] = (byte)item.Length;
                for (int i = 0; i < item.Length; i++)
                {
                    tester[offset++] = (byte)item[i];
                }
            }
            return tester;
        }
        public static byte[] ToByteArray(this Bonjour.mDNSResourcePoco resource, byte[] labelCache)
        {

            System.IO.BinaryWriter bw = new(new System.IO.MemoryStream());
            bw.Write(EncodeName(resource.Name, labelCache));
            bw.WriteU16BE((ushort)(int)resource.ResourceType);
            bw.WriteU16BE((ushort)(int)resource.ResourceClass);
            if (resource.Category != mDNSResourceCategory.Question)
            {
                bw.WriteU32BE(resource.TTL);
                switch (resource.ResourceType)
                {
                    case mDNSResourceTypes.A:
                        {
                            bw.WriteU16BE(4);
                            bw.Write(resource.A_IPAddress.GetAddressBytes());
                        }
                        break;
                    case mDNSResourceTypes.AAAA:
                        {
                            bw.WriteU16BE(16);
                            bw.Write(resource.A_IPAddress.GetAddressBytes());
                        }
                        break;
                    case mDNSResourceTypes.SRV:
                        byte[] srvBytes = EncodeName(resource.SRV_Target, labelCache.Concat((bw.BaseStream as System.IO.MemoryStream).ToArray()).ToArray());
                        bw.WriteU16BE((ushort)(srvBytes.Length + 6));
                        bw.WriteU16BE(resource.SRV_Priority);
                        bw.WriteU16BE(resource.SRV_Weight);
                        bw.WriteU16BE(resource.SRV_Port);
                        bw.Write(srvBytes);
                        break;
                    case mDNSResourceTypes.PTR:
                        byte[] ptrBytes = EncodeName(resource.PTR_DomainNamePointer,labelCache.Concat((bw.BaseStream as System.IO.MemoryStream).ToArray()).ToArray());
                        bw.WriteU16BE((ushort)ptrBytes.Length);
                        bw.Write(ptrBytes);
                        break;
                    case mDNSResourceTypes.TXT:
                        int TXTlen = resource.TXT.Count + resource.TXT.Aggregate((a, b) => a + b).Length;
                        bw.WriteU16BE((ushort)TXTlen);
                        foreach (var item in resource.TXT)
                        {
                            bw.Write(item);//we do not attempt to shorten this.
                        }
                        break;
                    case mDNSResourceTypes.NSEC:
                        break;
                    default:
                        bw.WriteU16BE((ushort)resource.ResourceData.Length);
                        bw.Write(resource.ResourceData);
                        break;

                }
            }
            return (bw.BaseStream as System.IO.MemoryStream).ToArray();
        }

        private static byte[] EncodeName(string Name, byte[] labelCache)
        {
            List<string[]> testStrings = new();
            string[] labels = (Name.TrimEnd('.') + ".").Split('.');
            int length = labels.Length - 1;
            byte[] finalist = LabelsToByte(labels);
            for (int i = 0; i < length; i++)
            {
                testStrings.Add(labels);
                labels = labels.Skip(1).ToArray();
            }
            foreach (var testString in testStrings)
            {
                var tester = LabelsToByte(testString);
                if (labelCache.PatternAt(tester).Any())
                {
                    int LabelRef = 0xC000 + labelCache.PatternAt(tester).First();
                    finalist = finalist[..^tester.Length].Concat(new byte[] { (byte)(LabelRef >> 8), (byte)(LabelRef & 0xff) }).ToArray();
                    break;
                }
            }
            return finalist;
        }
    }
}
