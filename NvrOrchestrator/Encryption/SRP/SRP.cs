using System;
using static NvrOrchestrator.Encryption.SRP.SrpBigInteger;

#pragma warning disable IDE1006 // Naming Styles

namespace NvrOrchestrator.Encryption.SRP
{
    //For someone reading this code, please note I overloaded SrpBigInteger for maximum readability, but NOT in conformance with the typical symbol's use.
    //| (pipe) means concatenate, not OR.
    //% still means MOD
    //^ still means pow, but includes mod (modpow)
    public class SRP
    {

        //for the most ideal property use, some properties cannot be automatic. These backing fields are for those circumstances.
        private SrpBigInteger _A;
        private SrpBigInteger _B;
        private SrpBigInteger _M1;
        private SrpBigInteger _M2;

        public SRP(SrpBigInteger N, SrpBigInteger g)
        {
            this.N = N;
            this.g = g;

            SetGlobalN(N);
        }
        public SrpBigInteger N { get; private set; }
        public SrpBigInteger g { get; private set; }
        public SrpBigInteger k
        {
            get
            {
                return H(N | Pad(g));
            }
        }
        public SrpBigInteger a { private get; set; }
        public SrpBigInteger b { private get; set; }
        public SrpBigInteger s { get; set; }
        public SrpBigInteger I { private get; set; }
        public SrpBigInteger p { private get; set; }
        public SrpBigInteger x
        {
            get
            {
                //check for s, I, p before returning.
                if (s == null || I == null || p == null)
                    return null;
                var xx = H(s | H(I | new SrpBigInteger(":") | p));
                v = g ^ xx;
                return xx;
            }
        }
        public SrpBigInteger v { get; set; }
        public SrpBigInteger A
        {
            get
            {
                if (_A == null && a != null)
                {
                    _A = g ^ a;
                }
                return _A;
            }
            set
            {
                _A = value;
            }
        }
        public SrpBigInteger B
        {
            get
            {
                if (_B == null && b != null && v != null)
                {
                    _B = (k * v + (g ^ b)) % N;
                }
                return _B;
            }
            set
            {
                _B = value;
            }
        }
        public SrpBigInteger u
        {
            get
            {
                return H(Pad(A) | Pad(B));
            }
        }
        public SrpBigInteger S1
        {
            get
            {
                return B - k * (g ^ x) ^ a + u * x;
            }
        }
        public SrpBigInteger S2
        {
            get
            {
                return A * (v ^ u) ^ b;
            }
        }
        public SrpBigInteger K1
        {
            get
            {
                try
                {
                    return H(S1);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public SrpBigInteger K2
        {
            get
            {
                try
                {
                    return H(S2);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public SrpBigInteger M1
        {
            get
            {
                if (_M1 == null)
                {
                    var K = K1 ?? K2;
                    return H(XOR(H(N), H(g)) | H(I) | s | A | B | K);
                }
                else
                {
                    return _M1;
                }
            }
            set
            {
                _M1 = value;
            }
        }
        public SrpBigInteger M2
        {
            get
            {
                if (_M2 == null)
                {
                    var K = K1 ?? K2;
                    return H(A | M1 | K);
                }
                else
                {
                    return _M2;
                }
            }
            set
            {
                _M2 = value;
            }
        }

    }
}
