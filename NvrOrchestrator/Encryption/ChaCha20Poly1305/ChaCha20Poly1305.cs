using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.Encryption.ChaCha20Poly1305
{
    public static class ChaCha20Poly1305
    {
        public static byte[] ChaChaEncryptHelper(byte[] plainText, byte[] sharedSecret, string nonce, string salt, string info, byte[] aad = null)
        {
            var key = System.Security.Cryptography.HKDF.DeriveKey(
                System.Security.Cryptography.HashAlgorithmName.SHA512,
                sharedSecret,
                32,
                Encoding.UTF8.GetBytes(salt),
                Encoding.UTF8.GetBytes(info));
            return ChaChaEncryptHelper(key, plainText, Encoding.UTF8.GetBytes(nonce.PadLeft(12, '\0')), aad);
        }
        public static byte[] ChaChaEncryptHelper(byte[] key, byte[] plainText, string nonce, byte[] aad = null)
        {
            return ChaChaEncryptHelper(key, plainText, Encoding.UTF8.GetBytes(nonce.PadLeft(12, '\0')), aad);
        }
        public static byte[] ChaChaEncryptHelper(byte[] key, byte[] plainText, byte[] nonce, byte[] aad = null)
        {
            var crypText = ChaChaPolyManager.ChaCha20Poly1305_Encrypt(nonce, plainText, aad, key, out byte[] tag);
            return crypText.Concat(tag).ToArray();
        }
    }
}
