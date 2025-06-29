#if ANTI_CHEAT
#if (UNITY_WINRT || UNITY_WINRT_10_0 || UNITY_WSA || UNITY_WSA_10_0) && !ENABLE_IL2CPP
#define NO_IL2CPP
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.Core
{

    public struct TSByte : IFormattable, IEquatable<TSByte>, IComparable<TSByte>, IComparable<byte>, IComparable
    {
        private static byte cryptoKey = 244;
        private byte currentCryptoKey;
        private bool inited;

        private byte hiddenValue;
#if DETECTOR
        private byte fakeValue;
        private bool fakeValueActive;
#endif
        private TSByte(byte value)
        {
            currentCryptoKey = (byte)ThreadSafeRandom.Next(byte.MaxValue);
            hiddenValue = EncryptDecrypt(value, currentCryptoKey);
#if DETECTOR
#if UNITY_EDITOR
            fakeValue = value;
            fakeValueActive = true;
#else
            var detectorRunning = ObscuredCheatingDetector.ExistsAndIsRunning;
            fakeValue = detectorRunning ? value : (byte)0;
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
        public static byte EncryptDecrypt(byte value)
        {
            return EncryptDecrypt(value, 0);
        }
        /// <summary>
        /// 简单的对称加密，使用默认的加密密钥。
        /// </summary>
        /// <param name="value"></param>
        public static void EncryptDecrypt(byte[] value)
        {
            EncryptDecrypt(value, 0);
        }
        /// <summary>
        /// 简单的对称加密，使用默认的加密密钥。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte EncryptDecrypt(byte value, byte key)
        {
            if (key == 0)
            {
                return (byte)(value ^ cryptoKey);
            }
            return (byte)(value ^ key);
        }
        /// <summary>
        /// 简单的对称加密，使用默认的加密密钥。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        public static void EncryptDecrypt(byte[] value, byte key)
        {
            var len = value.Length;
            for (var i = 0; i < len; i++)
            {
                if (key == 0)
                {
                    value[i] = (byte)(value[i] ^ cryptoKey);
                }
                else
                {
                    value[i] = (byte)(value[i] ^ key);
                }
            }
        }

        private byte InternalDecrypt()
        {
            if (!inited)
            {
                currentCryptoKey = (byte)ThreadSafeRandom.Next(byte.MaxValue);
                hiddenValue = EncryptDecrypt(0, currentCryptoKey);
#if DETECTOR
                fakeValue = 0;
                fakeValueActive = false;
#endif
                inited = true;

                return 0;
            }

            var decrypted = EncryptDecrypt(hiddenValue, currentCryptoKey);
#if DETECTOR
            if (ObscuredCheatingDetector.ExistsAndIsRunning && fakeValueActive && decrypted != fakeValue)
            {
                ObscuredCheatingDetector.Instance.OnCheatingDetected();
            }
#endif

            return decrypted;
        }

        #region overrides

        public static implicit operator TSByte(byte value)
        {
            return new TSByte(value);
        }

        public static implicit operator byte(TSByte value)
        {
            return value.InternalDecrypt();
        }
        public static TSByte operator ++(TSByte input)
        {
            var decrypted = (byte)(input.InternalDecrypt() + 1);
            input.hiddenValue = EncryptDecrypt(decrypted, input.currentCryptoKey);
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

        public static TSByte operator --(TSByte input)
        {
            var decrypted = (byte)(input.InternalDecrypt() - 1);
            input.hiddenValue = EncryptDecrypt(decrypted, input.currentCryptoKey);
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
        public static bool operator ==(TSByte left, TSByte right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(TSByte left, TSByte right)
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
            if (!(obj is TSByte))
                return false;
            return Equals((TSByte)obj);
        }
        public bool Equals(TSByte obj)
        {
            if (currentCryptoKey == obj.currentCryptoKey)
            {
                return hiddenValue == obj.hiddenValue;
            }

            return EncryptDecrypt(hiddenValue, currentCryptoKey) == EncryptDecrypt(obj.hiddenValue, obj.currentCryptoKey);
        }
        /// <summary>
        /// 将此实例与指定的 8 位无符号整数进行比较并返回对其相对值的指示。
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(TSByte other)
        {
            return InternalDecrypt().CompareTo(other.InternalDecrypt());
        }

        public int CompareTo(byte other)
        {
            return InternalDecrypt().CompareTo(other);
        }

        public int CompareTo(object obj)
        {
#if !NO_IL2CPP
            return InternalDecrypt().CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is byte)) throw new ArgumentException("Argument must be byte");
			return CompareTo((byte)obj);
#endif
        }
        #endregion

    }
}

#endif