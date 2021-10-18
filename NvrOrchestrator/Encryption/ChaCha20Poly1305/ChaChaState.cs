using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NvrOrchestrator.Encryption.ChaCha20Poly1305
{
    public struct ChaChaState
    {
        uint T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15;
        public ChaChaState(byte[] key, uint counter, byte[] nonce)
        {
            T0 = 0x61707865;
            T1 = 0x3320646e;
            T2 = 0x79622d32;
            T3 = 0x6b206574;
            T4 = BitConverter.ToUInt32(key, 0);
            T5 = BitConverter.ToUInt32(key, 4);
            T6 = BitConverter.ToUInt32(key, 8);
            T7 = BitConverter.ToUInt32(key, 12);
            T8 = BitConverter.ToUInt32(key, 16);
            T9 = BitConverter.ToUInt32(key, 20);
            T10 = BitConverter.ToUInt32(key, 24);
            T11 = BitConverter.ToUInt32(key, 28);
            T12 = counter;
            T13 = BitConverter.ToUInt32(nonce, 0);
            T14 = BitConverter.ToUInt32(nonce, 4);
            T15 = BitConverter.ToUInt32(nonce, 8);
        }

        public ChaChaState Copy()
        {
            return new ChaChaState()
            {
                T0 = T0,
                T1 = T1,
                T2 = T2,
                T3 = T3,
                T4 = T4,
                T5 = T5,
                T6 = T6,
                T7 = T7,
                T8 = T8,
                T9 = T9,
                T10 = T10,
                T11 = T11,
                T12 = T12,
                T13 = T13,
                T14 = T14,
                T15 = T15,
            };
        }

        public void Perform20Rounds()
        {
            for (int i = 0; i < 10; i++)
            {
                //QR(T0,T4,T8,T12)
                QR(ref T0, ref T4, ref T8, ref T12);
                //QR(T1,T5,T9,T113)
                QR(ref T1, ref T5, ref T9, ref T13);
                //QR(T2,T5,T9,T13)
                QR(ref T2, ref T6, ref T10, ref T14);
                //QR(T3,T7,T11,T15)
                QR(ref T3, ref T7, ref T11, ref T15);

                //QR(T0,T5,T10,T15)
                QR(ref T0, ref T5, ref T10, ref T15);
                //QR(T1,T6,T11,T12)
                QR(ref T1, ref T6, ref T11, ref T12);
                //QR(T2,T7,T8,T13)
                QR(ref T2, ref T7, ref T8, ref T13);
                //QR(T3,T4,T9,T14)
                QR(ref T3, ref T4, ref T9, ref T14);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void QR(ref uint a, ref uint b, ref uint c, ref uint d)
        {
            unchecked
            {
                a += b; d ^= a; d = ROTL(d, 16);
                c += d; b ^= c; b = ROTL(b, 12);
                a += b; d ^= a; d = ROTL(d, 8);
                c += d; b ^= c; b = ROTL(b, 7);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint ROTL(uint a, byte b)
        {
            return a << b | a >> 32 - b;
        }

        public static ChaChaState operator +(ChaChaState first, ChaChaState second)
        {
            unchecked
            {
                return new ChaChaState()
                {
                    T0 = first.T0 + second.T0,
                    T1 = first.T1 + second.T1,
                    T2 = first.T2 + second.T2,
                    T3 = first.T3 + second.T3,
                    T4 = first.T4 + second.T4,
                    T5 = first.T5 + second.T5,
                    T6 = first.T6 + second.T6,
                    T7 = first.T7 + second.T7,
                    T8 = first.T8 + second.T8,
                    T9 = first.T9 + second.T9,
                    T10 = first.T10 + second.T10,
                    T11 = first.T11 + second.T11,
                    T12 = first.T12 + second.T12,
                    T13 = first.T13 + second.T13,
                    T14 = first.T14 + second.T14,
                    T15 = first.T15 + second.T15,
                };
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SpeedyIntWrite(byte[] a, uint b, int offset)
        {
            a[offset] = (byte)b;
            a[offset + 1] = (byte)(b >> 8);
            a[offset + 2] = (byte)(b >> 16);
            a[offset + 3] = (byte)(b >> 24);
        }

        public static explicit operator byte[](ChaChaState state)
        {
            var retVal = new byte[64];
            SpeedyIntWrite(retVal, state.T0, 0);
            SpeedyIntWrite(retVal, state.T1, 4);
            SpeedyIntWrite(retVal, state.T2, 8);
            SpeedyIntWrite(retVal, state.T3, 12);
            SpeedyIntWrite(retVal, state.T4, 16);
            SpeedyIntWrite(retVal, state.T5, 20);
            SpeedyIntWrite(retVal, state.T6, 24);
            SpeedyIntWrite(retVal, state.T7, 28);
            SpeedyIntWrite(retVal, state.T8, 32);
            SpeedyIntWrite(retVal, state.T9, 36);
            SpeedyIntWrite(retVal, state.T10, 40);
            SpeedyIntWrite(retVal, state.T11, 44);
            SpeedyIntWrite(retVal, state.T12, 48);
            SpeedyIntWrite(retVal, state.T13, 52);
            SpeedyIntWrite(retVal, state.T14, 56);
            SpeedyIntWrite(retVal, state.T15, 60);

            return retVal;

        }
    }
}
