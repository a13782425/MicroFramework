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
    
    public struct TSUShort : IFormattable, IEquatable<TSUShort>, IComparable<TSUShort>, IComparable<ushort>, IComparable
    {
        private static ushort cryptoKey = 224;

        private ushort currentCryptoKey;
        private ushort hiddenValue;
        private bool inited;
#if DETECTOR
        private ushort fakeValue;
        private bool fakeValueActive;
#endif

        private TSUShort(ushort value)
        {
            currentCryptoKey = (ushort)ThreadSafeRandom.Next(short.MaxValue);
            hiddenValue = EncryptDecrypt(value, currentCryptoKey);
#if DETECTOR
#if UNITY_EDITOR
            fakeValue = value;
            fakeValueActive = true;
#else
            var detectorRunning = ObscuredCheatingDetector.ExistsAndIsRunning;
            fakeValue = detectorRunning ? value : (ushort)0;
            fakeValueActive = detectorRunning;
#endif
#endif

            inited = true;
        }

        /// <summary>
        /// 简单的对称加解密，使用默认的加密密钥。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ushort EncryptDecrypt(ushort value)
        {
            return EncryptDecrypt(value, 0);
        }

        /// <summary>
        /// 简单的对称加解密，使用默认的加密密钥。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static ushort EncryptDecrypt(ushort value, ushort key)
        {
            if (key == 0)
            {
                return (ushort)(value ^ cryptoKey);
            }
            return (ushort)(value ^ key);
        }

        private ushort InternalDecrypt()
        {
            if (!inited)
            {
                currentCryptoKey = (ushort)ThreadSafeRandom.Next(short.MaxValue);
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

        public static implicit operator TSUShort(ushort value)
        {
            return new TSUShort(value);
        }

        public static implicit operator ushort(TSUShort value)
        {
            return value.InternalDecrypt();
        }

        public static TSUShort operator ++(TSUShort input)
        {
            var decrypted = (ushort)(input.InternalDecrypt() + 1);
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

        public static TSUShort operator --(TSUShort input)
        {
            var decrypted = (ushort)(input.InternalDecrypt() - 1);
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
        public static bool operator ==(TSUShort left, TSUShort right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(TSUShort left, TSUShort right)
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
            if (!(obj is TSUShort))
                return false;
            return Equals((TSUShort)obj);
        }

        public bool Equals(TSUShort obj)
        {
            if (currentCryptoKey == obj.currentCryptoKey)
            {
                return hiddenValue == obj.hiddenValue;
            }

            return EncryptDecrypt(hiddenValue, currentCryptoKey) == EncryptDecrypt(obj.hiddenValue, obj.currentCryptoKey);
        }

        public int CompareTo(TSUShort other)
        {
            return InternalDecrypt().CompareTo(other.InternalDecrypt());
        }

        public int CompareTo(ushort other)
        {
            return InternalDecrypt().CompareTo(other);
        }

        public int CompareTo(object obj)
        {
#if !NO_IL2CPP
            return InternalDecrypt().CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is ushort)) throw new ArgumentException("Argument must be ushort");
			return CompareTo((ushort)obj);
#endif
        }

        #endregion

    }
}

#endif