#if ANTI_CHEAT
#if (UNITY_WINRT || UNITY_WINRT_10_0 || UNITY_WSA || UNITY_WSA_10_0) && !ENABLE_IL2CPP
#define NO_IL2CPP
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace MFramework.Core
{
    public struct TSDouble : IFormattable, IEquatable<TSDouble>, IComparable<TSDouble>, IComparable<double>, IComparable
    {
        private static long cryptoKey = 210987L;


        private long currentCryptoKey;


        private bool inited;


        private long hiddenValue;


#pragma warning disable 414
        private Byte8 hiddenValueOldByte8;
#pragma warning restore 414

#if DETECTOR


        private double fakeValue;


        private bool fakeValueActive;
#endif

        private TSDouble(double value)
        {
            currentCryptoKey = ThreadSafeRandom.Next(100000, 999999);
            hiddenValue = InternalEncrypt(value, currentCryptoKey);
            hiddenValueOldByte8 = default(Byte8);

#if DETECTOR
#if UNITY_EDITOR
            fakeValue = value;
            fakeValueActive = true;
#else
            var detectorRunning = ObscuredCheatingDetector.ExistsAndIsRunning;
            fakeValue = detectorRunning ? value : 0L;
            fakeValueActive = detectorRunning;
#endif
#endif
            inited = true;
        }

        /// <summary>
        /// 简单的对称加密，使用默认的加密密钥。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long Encrypt(double value)
        {
            return Encrypt(value, cryptoKey);
        }

        /// <summary>
        /// 简单的对称加密，使用默认的加密密钥。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long Encrypt(double value, long key)
        {
            var u = new DoubleLongBytesUnion { d = value };
            u.l = u.l ^ key;
            u.b8.Shuffle();
            return u.l;
        }

        private static long InternalEncrypt(double value, long key = 0L)
        {
            var currentKey = key;
            if (currentKey == 0L)
            {
                currentKey = cryptoKey;
            }

            return Encrypt(value, currentKey);
        }

        /// <summary>
        /// 解密
        /// </summary>
        public static double Decrypt(long value)
        {
            return Decrypt(value, cryptoKey);
        }

        /// <summary>
        /// 解密
        /// </summary>
        public static double Decrypt(long value, long key)
        {
            var u = new DoubleLongBytesUnion { l = value };
            u.b8.UnShuffle();
            u.l ^= key;
            return u.d;
        }


        private double InternalDecrypt()
        {
            if (!inited)
            {
                currentCryptoKey = ThreadSafeRandom.Next(100000, 999999);
                hiddenValue = InternalEncrypt(0, currentCryptoKey);
#if DETECTOR
                fakeValue = 0;
                fakeValueActive = false;
#endif
                inited = true;

                return 0;
            }

            var decrypted = Decrypt(hiddenValue, currentCryptoKey);
#if DETECTOR
            if (ObscuredCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue) > ObscuredCheatingDetector.Instance.DoubleEpsilon)
            {
                ObscuredCheatingDetector.Instance.OnCheatingDetected();
            }
#endif

            return decrypted;
        }

        #region operators, overrides, interface implementations

        //! @cond
        public static implicit operator TSDouble(double value)
        {
            return new TSDouble(value);
        }

        public static implicit operator double(TSDouble value)
        {
            return value.InternalDecrypt();
        }

        public static implicit operator TSInt(TSDouble value)
        {
            return (int)value.InternalDecrypt();
        }

        public static explicit operator TSDouble(TSFloat f)
        {
            return (float)f;
        }

        public static TSDouble operator ++(TSDouble input)
        {
            var decrypted = input.InternalDecrypt() + 1d;
            input.hiddenValue = InternalEncrypt(decrypted, input.currentCryptoKey);
#if DETECTOR
            if (ObscuredCheatingDetector.ExistsAndIsRunning)
            {
                input.fakeValue = decrypted;
                input.fakeValueActive = true;
            }
            else
            {
                input.fakeValueActive = false;
            }
#endif

            return input;
        }

        public static TSDouble operator --(TSDouble input)
        {
            var decrypted = input.InternalDecrypt() - 1d;
            input.hiddenValue = InternalEncrypt(decrypted, input.currentCryptoKey);
#if DETECTOR
            if (ObscuredCheatingDetector.ExistsAndIsRunning)
            {
                input.fakeValue = decrypted;
                input.fakeValueActive = true;
            }
            else
            {
                input.fakeValueActive = false;
            }
#endif

            return input;
        }
        public static bool operator ==(TSDouble left, TSDouble right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(TSDouble left, TSDouble right)
        {
            return !left.Equals(right);
        }
        public override int GetHashCode()
        {
            return InternalDecrypt().GetHashCode();
        }

        public override string ToString()
        {
            return InternalDecrypt().ToString();
        }

        public string ToString(string format)
        {
            return InternalDecrypt().ToString(format);
        }

        public string ToString(IFormatProvider provider)
        {
            return InternalDecrypt().ToString(provider);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            return InternalDecrypt().ToString(format, provider);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TSDouble))
                return false;
            return Equals((TSDouble)obj);
        }

        public bool Equals(TSDouble obj)
        {
            return obj.InternalDecrypt().Equals(InternalDecrypt());
        }

        public int CompareTo(TSDouble other)
        {
            return InternalDecrypt().CompareTo(other.InternalDecrypt());
        }

        public int CompareTo(double other)
        {
            return InternalDecrypt().CompareTo(other);
        }

        public int CompareTo(object obj)
        {
#if !NO_IL2CPP
            return InternalDecrypt().CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is double)) throw new ArgumentException("Argument must be double");
			return CompareTo((double)obj);
#endif
        }
        #endregion

        [StructLayout(LayoutKind.Explicit)]
        private struct DoubleLongBytesUnion
        {
            [FieldOffset(0)]
            public double d;

            [FieldOffset(0)]
            public long l;

            [FieldOffset(0)]
            public Byte8 b8;
        }
    }
}

#endif