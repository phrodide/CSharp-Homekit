using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace NvrOrchestrator.Encryption.ChaCha20Poly1305
{
    public static class ChaChaPolyManager
    {
        public static byte[] Encrypt(byte[] key, byte[] nonce, uint counter, byte[] input)
        {
            System.IO.MemoryStream mi = new System.IO.MemoryStream(input);
            System.IO.MemoryStream mo = new System.IO.MemoryStream();
            while (mi.Position != mi.Length)
            {
                var startingPosition = mi.Position;
                ChaChaState state = new(key, counter++, nonce);
                ChaChaState x = state.Copy();
                x.Perform20Rounds();
                var tmp = (byte[])(x + state);
                for (int i = 0; i < Math.Min(mi.Length - startingPosition, 64); i++)
                {
                    mo.WriteByte((byte)(mi.ReadByte() ^ tmp[i]));
                }

            }

            return mo.ToArray();
        }

        public static byte[] Decrypt(byte[] key, byte[] nonce, uint counter, byte[] input)
        {
            return Encrypt(key, nonce, counter, input);
        }


        public static byte[] CreateTag(PolyState self, byte[] data)
        {
            byte[] n = new byte[17];
            System.IO.MemoryStream mi = new(data);
            int rounds = data.Length / 16 + (data.Length % 16 != 0 ? 1 : 0);
            for (int i = 0; i < rounds; i++)
            {
                int numRead = mi.Read(n, 0, 16);
                n[numRead] = 1;
                for (int j = 0; j < 16 - numRead; j++)
                {
                    n[numRead + j + 1] = 0;//This line is unnecessary in ChaCha20 as the blocks are aligned to 16 bytes.
                }
                self.acc += new BigInteger(n, isUnsigned: true);
                self.acc = self.r * self.acc % PolyState.P;
            }
            self.acc += self.s;
            return self.acc.ToByteArray(isUnsigned: true).Take(16).ToArray();
        }

        public static byte[] ChaCha20Poly1305_Encrypt(byte[] nonce, byte[] plaintext, byte[] aad, byte[] key, out byte[] tag)
        {
            //generate poly key by ChaCha20 with counter=0
            if (aad == null) aad = new byte[0];
            byte[] PolyKey = Encrypt(key, nonce, 0, new byte[32]);
            PolyState poly = new(PolyKey);
            //chacha20 encrypt the data with counter=1
            var crypText = Decrypt(key, nonce, 1, plaintext);
            //generate the auth data stream by concatenating: aad (padded to 16 bytes), encryptedText (padded to 16 bytes) 8 byte original aad len, 8 byte original encryptedText len
            ulong AadPadLen = (ulong)((16 - aad.Length % 16) % 16);
            ulong CryptTextPadLen = (ulong)((16 - crypText.Length % 16) % 16);
            var polyData = aad.Concat(new byte[AadPadLen]).Concat(crypText).Concat(new byte[CryptTextPadLen]).Concat(BitConverter.GetBytes((ulong)aad.Length)).Concat(BitConverter.GetBytes((ulong)crypText.Length)).ToArray();
            //create tag from auth data stream.
            tag = CreateTag(poly, polyData); ;
            return crypText;
        }
        public static byte[] ChaCha20Poly1305_Decrypt(byte[] nonce, byte[] cryptext, byte[] aad, byte[] key, byte[] tag)
        {
            if (aad == null) aad = new byte[0];
            //generate poly key by ChaCha20 with counter=0
            byte[] PolyKey = Encrypt(key, nonce, 0, new byte[32]);
            PolyState poly = new(PolyKey);
            ulong AadPadLen = (ulong)((16 - aad.Length % 16) % 16);
            ulong CryptTextPadLen = (ulong)((16 - cryptext.Length % 16) % 16);
            //generate the auth data stream by concatenating: aad (padded to 16 bytes), encryptedText (padded to 16 bytes) 8 byte original aad len, 8 byte original encryptedText len
            var polyData = aad.Concat(new byte[AadPadLen]).Concat(cryptext).Concat(new byte[CryptTextPadLen]).Concat(BitConverter.GetBytes((ulong)aad.Length)).Concat(BitConverter.GetBytes((ulong)cryptext.Length)).ToArray();
            //create tag from auth data stream.
            var verifyTag = CreateTag(poly, polyData);
            //verify tag. If proper, chacha20 decrypt the data with counter=1
            for (int i = 0; i < 16; i++)
            {
                if (verifyTag[i] != tag[i])
                {
                    throw new Exception("Bad Verification");
                }
            }
            var plainText = Decrypt(key, nonce, 1, cryptext);
            return plainText;
        }
    }
}
