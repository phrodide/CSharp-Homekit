using System.Security.Cryptography;

namespace NvrOrchestrator.Encryption.Curve25519
{
    public class Keys
    {
        internal byte[] _ownPrivate = null;
        byte[] _sharedSecret = null;
        public byte[] ownPrivate
        {
            set
            {
                if (value == null)
                {
                    var privateKey = new byte[32];
                    RandomNumberGenerator.Create().GetBytes(privateKey);
                    _ownPrivate = privateKey;
                }
                else
                {
                    _ownPrivate = value;
                }
                _ownPrivate[31] &= 0x7F;
                _ownPrivate[31] |= 0x40;
                _ownPrivate[0] &= 0xF8;
                ownPublic = NaCl.MontgomeryCurve25519.GetPublicKey(_ownPrivate);
            }
        }
        public byte[] ownPublic { get; private set; }
        public byte[] remotePublic { get; set; }
        public byte[] sharedSecret
        {
            get
            {
                if (_sharedSecret == null && _ownPrivate != null && remotePublic != null)
                {
                    _sharedSecret = NaCl.MontgomeryCurve25519.KeyExchange(remotePublic, _ownPrivate);
                }
                return _sharedSecret;
            }
        }
    }
}
