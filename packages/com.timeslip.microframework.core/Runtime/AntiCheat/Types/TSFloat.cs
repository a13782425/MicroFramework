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
    public struct TSFloat : IFormattable, IEquatable<TSFloat>, IComparable<TSFloat>, IComparable<float>, IComparable
    {
        private static int cryptoKey = 230887;


        private int currentCryptoKey;


        private int hiddenValue;


        [FormerlySerializedAs("hiddenValue")]
#pragma warning disable 414
        private Byte4 hiddenValueOldByte4;
#pragma warning restore 414


        private bool inited;

#if DETECTOR

        private float fakeValue;


        private bool fakeValueActive;
#endif

        private TSFloat(float value)
        {
            currentCryptoKey = ThreadSafeRandom.Next(100000, 999999);
            hiddenValue = InternalEncrypt(value, currentCryptoKey);
            hiddenValueOldByte4 = default(Byte4);
#if DETECTOR
#if UNITY_EDITOR
            fakeValue = value;
            fakeValueActive = true;
#else
            var detectorRunning = ObscuredCheatingDetector.ExistsAndIsRunning;
            fakeValue = detectorRunning ? value : 0f;
            fakeValueActive = detectorRunning;
#endif
#endif

            inited = true;
        }

        /// <summary>
        /// 设置新的Key
        /// </summary>
        /// <param name="newKey"></param>
        public static void SetNewCryptoKey(int newKey)
        {
            cryptoKey = newKey;
        }

        /// <summary>
        /// 简单的对称加密，使用默认的加密密钥。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Encrypt(float value)
        {
            return Encrypt(value, cryptoKey);
        }

        /// <summary>
        /// 简单的对称加密，使用默认的加密密钥。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int Encrypt(float value, int key)
        {
            var u = new FloatIntBytesUnion { f = value };
            u.i = u.i ^ key;
            u.b4.Shuffle();
            return u.i;
        }

        private static int InternalEncrypt(float value, int key = 0)
        {
            var currentKey = key;
            if (currentKey == 0)
            {
                currentKey = cryptoKey;
            }

            return Encrypt(value, currentKey);
        }

        /// <summary>
        /// 解密
        /// </summary>
        public static float Decrypt(int value)
        {
            return Decrypt(value, cryptoKey);
        }

        /// <summary>
        /// 解密
        /// </summary>
        public static float Decrypt(int value, int key)
        {
            var u = new FloatIntBytesUnion { i = value };
            u.b4.UnShuffle();
            u.i ^= key;
            return u.f;
        }

        private float InternalDecrypt()
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
            if (ObscuredCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue) > ObscuredCheatingDetector.Instance.FloatEpsilon)
            {
                ObscuredCheatingDetector.Instance.OnCheatingDetected();
            }
#endif

            return decrypted;
        }

        #region overrides

        public static implicit operator TSFloat(float value)
        {
            return new TSFloat(value);
        }

        public static implicit operator float(TSFloat value)
        {
            return value.InternalDecrypt();
        }

        public static TSFloat operator ++(TSFloat input)
        {
            var decrypted = input.InternalDecrypt() + 1f;
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

        public static TSFloat operator --(TSFloat input)
        {
            var decrypted = input.InternalDecrypt() - 1f;
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
        public static bool operator ==(TSFloat left, TSFloat right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(TSFloat left, TSFloat right)
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
            if (!(obj is TSFloat))
                return false;
            return Equals((TSFloat)obj);
        }

        public bool Equals(TSFloat obj)
        {
            return obj.InternalDecrypt().Equals(InternalDecrypt());
        }

        public int CompareTo(TSFloat other)
        {
            return InternalDecrypt().CompareTo(other.InternalDecrypt());
        }

        public int CompareTo(float other)
        {
            return InternalDecrypt().CompareTo(other);
        }

        public int CompareTo(object obj)
        {
#if !NO_IL2CPP
            return InternalDecrypt().CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is float)) throw new ArgumentException("Argument must be float");
			return CompareTo((float)obj);
#endif
        }
        #endregion

        [StructLayout(LayoutKind.Explicit)]
        internal struct FloatIntBytesUnion
        {
            [FieldOffset(0)]
            public float f;

            [FieldOffset(0)]
            public int i;

            [FieldOffset(0)]
            public Byte4 b4;
        }
    }
}

#endif