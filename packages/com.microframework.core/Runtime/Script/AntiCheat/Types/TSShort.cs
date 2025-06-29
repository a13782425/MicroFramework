#if ANTI_CHEAT
#if (UNITY_WINRT || UNITY_WINRT_10_0 || UNITY_WSA || UNITY_WSA_10_0) && !ENABLE_IL2CPP
#define NO_IL2CPP
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Core
{
    
    public struct TSShort : IFormattable, IEquatable<TSShort>, IComparable<TSShort>, IComparable<short>, IComparable
    {
        private static short cryptoKey = 214;

        
        private short currentCryptoKey;

        
        private short hiddenValue;

        
        private bool inited;
#if DETECTOR
        
        private short fakeValue;

        
        private bool fakeValueActive;
#endif

        private TSShort(short value)
        {
            currentCryptoKey = (short)ThreadSafeRandom.Next(short.MaxValue);
            hiddenValue = EncryptDecrypt(value, currentCryptoKey);
#if DETECTOR
#if UNITY_EDITOR
            fakeValue = value;
            fakeValueActive = true;
#else
            var detectorRunning = ObscuredCheatingDetector.ExistsAndIsRunning;
            fakeValue = detectorRunning ? value : (short)0;
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
        public static short EncryptDecrypt(short value)
        {
            return EncryptDecrypt(value, 0);
        }

        /// <summary>
        /// 简单的对称加解密，使用默认的加密密钥。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static short EncryptDecrypt(short value, short key)
        {
            if (key == 0)
            {
                return (short)(value ^ cryptoKey);
            }
            return (short)(value ^ key);
        }

        private short InternalDecrypt()
        {
            if (!inited)
            {
                currentCryptoKey = (short)ThreadSafeRandom.Next(short.MaxValue);
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

        public static implicit operator TSShort(short value)
        {
            return new TSShort(value);
        }

        public static implicit operator short(TSShort value)
        {
            return value.InternalDecrypt();
        }

        public static TSShort operator ++(TSShort input)
        {
            var decrypted = (short)(input.InternalDecrypt() + 1);
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

        public static TSShort operator --(TSShort input)
        {
            var decrypted = (short)(input.InternalDecrypt() - 1);
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
        public static bool operator ==(TSShort left, TSShort right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(TSShort left, TSShort right)
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
            if (!(obj is TSShort))
                return false;
            return Equals((TSShort)obj);
        }

        public bool Equals(TSShort obj)
        {
            if (currentCryptoKey == obj.currentCryptoKey)
            {
                return hiddenValue == obj.hiddenValue;
            }

            return EncryptDecrypt(hiddenValue, currentCryptoKey) == EncryptDecrypt(obj.hiddenValue, obj.currentCryptoKey);
        }

        public int CompareTo(TSShort other)
        {
            return InternalDecrypt().CompareTo(other.InternalDecrypt());
        }

        public int CompareTo(short other)
        {
            return InternalDecrypt().CompareTo(other);
        }

        public int CompareTo(object obj)
        {
#if !NO_IL2CPP
            return InternalDecrypt().CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is short)) throw new ArgumentException("Argument must be short");
			return CompareTo((short)obj);
#endif
        }
        #endregion
    }
}

#endif