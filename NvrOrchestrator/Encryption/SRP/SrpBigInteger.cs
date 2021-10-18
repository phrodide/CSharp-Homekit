using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Security.Cryptography;

namespace NvrOrchestrator.Encryption.SRP
{
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public class SrpBigInteger
    {
        static BigInteger N = 0;//for modulus in modpow.
        public static void SetGlobalN(SrpBigInteger srpN) => N = new(srpN.backend, true, true);

        private byte[] backend;
        public SrpBigInteger(byte[] input)
        {
            backend = input;
        }
        public SrpBigInteger(string input)
        {
            backend = Encoding.UTF8.GetBytes(input);
        }

        public SrpBigInteger(int input)
        {
            backend = new BigInteger(input).ToByteArray(true, true);
        }

        public static SrpBigInteger operator |(SrpBigInteger a, SrpBigInteger b)
        {
            return new SrpBigInteger(a.backend.Concat(b.backend).ToArray());
        }

        public static SrpBigInteger operator %(SrpBigInteger a, SrpBigInteger b)
        {
            var Ba = new BigInteger(a.backend, true, true);
            var Bb = new BigInteger(b.backend, true, true);
            var Bc = Ba % Bb;
            return new SrpBigInteger(Bc.ToByteArray(true, true));
        }
        public static SrpBigInteger operator +(SrpBigInteger a, SrpBigInteger b)
        {
            var Ba = new BigInteger(a.backend, true, true);
            var Bb = new BigInteger(b.backend, true, true);
            var Bc = Ba + Bb;
            return new SrpBigInteger(Bc.ToByteArray(true, true));
        }
        public static SrpBigInteger operator -(SrpBigInteger a, SrpBigInteger b)
        {
            var Ba = new BigInteger(a.backend, true, true);
            var Bb = new BigInteger(b.backend, true, true);
            var Bc = Ba - Bb;
            return new SrpBigInteger(Bc.ToByteArray(true, true));
        }

        public static SrpBigInteger operator *(SrpBigInteger a, SrpBigInteger b)
        {
            var Ba = new BigInteger(a.backend, true, true);
            var Bb = new BigInteger(b.backend, true, true);
            var Bc = Ba * Bb;
            return new SrpBigInteger(Bc.ToByteArray(true, true));
        }
        public static SrpBigInteger operator ^(SrpBigInteger a, SrpBigInteger b)
        {
            var Ba = new BigInteger(a.backend, true, true);
            var Bb = new BigInteger(b.backend, true, true);
            var Bc = BigInteger.ModPow(Ba, Bb, N);
            return new SrpBigInteger(Bc.ToByteArray(true, true));
        }

        public static SrpBigInteger XOR(SrpBigInteger a, SrpBigInteger b)
        {
            var c = new SrpBigInteger(a.backend);
            for (int i = 0; i < c.backend.Length; i++)
            {
                c.backend[i] ^= b.backend[i];
            }
            return c;
        }

        public static SrpBigInteger Pad(SrpBigInteger a)
        {
            var padLength = N.ToByteArray(true, true).Length;
            var paddedBytes = new byte[padLength];
            Array.Copy(a.backend, 0, paddedBytes, padLength - a.backend.Length, a.backend.Length);
            return new(paddedBytes);
        }
        public static SrpBigInteger H(SrpBigInteger a)
        {
            return new(SHA512.Create().ComputeHash(a.backend));
        }

        public static bool operator ==(SrpBigInteger a, SrpBigInteger b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            return a.backend.SequenceEqual(b.backend);
        }
        public static bool operator !=(SrpBigInteger a, SrpBigInteger b)
        {
            if (ReferenceEquals(a, b))
                return false;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return true;
            return !a.backend.SequenceEqual(b.backend);
        }

        public byte[] ToByteArray()
        {
            return backend;
        }
    }
}
