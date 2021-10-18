using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace NvrOrchestrator.Encryption.ChaCha20Poly1305
{
    public struct PolyState
    {
        //0x0f ff ff fc, 0f ff ff fc, 0f ff ff fc, 0f ff ff ff
        private static BigInteger normalise_r = new BigInteger(new byte[] { 0xff, 0xff, 0xff, 0x0f, 0xfc, 0xff, 0xff, 0x0f, 0xfc, 0xff, 0xff, 0x0f, 0xfc, 0xff, 0xff, 0x0f });
        public static BigInteger P = new BigInteger(new byte[] { 0xfb, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x03 });//0x03 ff ff ff ff ff ff ff ff ff ff ff ff ff ff ff fb

        public PolyState(byte[] key)
        {
            acc = 0;
            r = new BigInteger(key.Take(16).ToArray(), true, false) & normalise_r;
            s = new BigInteger(key.Skip(16).ToArray(), true, false);
        }
        public BigInteger acc { get; set; }
        public BigInteger s { get; set; }
        public BigInteger r { get; set; }
    }
}
