using System;
using Chaos.NaCl.Internal.Ed25519Ref10;

namespace NaCl
{
    // This class is mainly for compatibility with NaCl's Curve25519 implementation
    // If you don't need that compatibility, use Ed25519.KeyExchange
    public static class MontgomeryCurve25519
    {
        public static readonly int PublicKeySizeInBytes = 32;
        public static readonly int PrivateKeySizeInBytes = 32;
        public static readonly int SharedKeySizeInBytes = 32;

        public static byte[] GetPublicKey(byte[] privateKey)
        {
            if (privateKey == null)
                throw new ArgumentNullException("privateKey");
            if (privateKey.Length != PrivateKeySizeInBytes)
                throw new ArgumentException("privateKey.Length must be 32");
            var publicKey = new byte[32];
            GetPublicKey(new ArraySegment<byte>(publicKey), new ArraySegment<byte>(privateKey));
            return publicKey;
        }

        static readonly byte[] _basePoint = new byte[32]
        {
            9, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0 ,0, 0, 0, 0, 0,
            0, 0, 0 ,0, 0, 0, 0, 0,
            0, 0, 0 ,0, 0, 0, 0, 0
        };

        public static void GetPublicKey(ArraySegment<byte> publicKey, ArraySegment<byte> privateKey)
        {
            if (publicKey.Array == null)
                throw new ArgumentNullException("publicKey.Array");
            if (privateKey.Array == null)
                throw new ArgumentNullException("privateKey.Array");
            if (publicKey.Count != PublicKeySizeInBytes)
                throw new ArgumentException("privateKey.Count must be 32");
            if (privateKey.Count != PrivateKeySizeInBytes)
                throw new ArgumentException("privateKey.Count must be 32");

            // hack: abusing publicKey as temporary storage
            // todo: remove hack
            for (int i = 0; i < 32; i++)
            {
                publicKey.Array[publicKey.Offset + i] = privateKey.Array[privateKey.Offset + i];
            }
            ScalarOperations.sc_clamp(publicKey.Array, publicKey.Offset);
            GroupOperations.ge_scalarmult_base(out GroupElementP3 A, publicKey.Array, publicKey.Offset);
            EdwardsToMontgomeryX(out FieldElement publicKeyFE, ref A.Y, ref A.Z);
            FieldOperations.fe_tobytes(publicKey.Array, publicKey.Offset, ref publicKeyFE);
        }

        private static readonly byte[] _zero16 = new byte[16];


        public static byte[] KeyExchange(byte[] publicKey, byte[] privateKey)
        {
            var sharedKey = new byte[SharedKeySizeInBytes];
            KeyExchange(new ArraySegment<byte>(sharedKey), new ArraySegment<byte>(publicKey), new ArraySegment<byte>(privateKey));
            return sharedKey;
        }

        public static void KeyExchange(ArraySegment<byte> sharedKey, ArraySegment<byte> publicKey, ArraySegment<byte> privateKey)
        {
            if (sharedKey.Array == null)
                throw new ArgumentNullException("sharedKey.Array");
            if (publicKey.Array == null)
                throw new ArgumentNullException("publicKey.Array");
            if (privateKey.Array == null)
                throw new ArgumentNullException("privateKey");
            if (sharedKey.Count != 32)
                throw new ArgumentException("sharedKey.Count != 32");
            if (publicKey.Count != 32)
                throw new ArgumentException("publicKey.Count != 32");
            if (privateKey.Count != 32)
                throw new ArgumentException("privateKey.Count != 32");
            MontgomeryOperations.scalarmult(sharedKey.Array, sharedKey.Offset, privateKey.Array, privateKey.Offset, publicKey.Array, publicKey.Offset);
        }

        internal static void EdwardsToMontgomeryX(out FieldElement montgomeryX, ref FieldElement edwardsY, ref FieldElement edwardsZ)
        {
            FieldElement tempX, tempZ;
            FieldOperations.fe_add(out tempX, ref edwardsZ, ref edwardsY);
            FieldOperations.fe_sub(out tempZ, ref edwardsZ, ref edwardsY);
            FieldOperations.fe_invert(out tempZ, ref tempZ);
            FieldOperations.fe_mul(out montgomeryX, ref tempX, ref tempZ);
        }
    }
}