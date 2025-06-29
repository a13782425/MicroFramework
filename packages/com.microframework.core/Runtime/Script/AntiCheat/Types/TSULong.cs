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
    
    public struct TSULong : IFormattable, IEquatable<TSULong>, IComparable<TSULong>, IComparable<ulong>, IComparable
    {
        private static ulong cryptoKey = 444443L;

        
        private ulong currentCryptoKey;

        
        private ulong hiddenValue;

        
        private bool inited;
#if DETECTOR
        
        private ulong fakeValue;

        
        private bool fakeValueActive;
#endif

        private TSULong(ulong value)
        {
            currentCryptoKey = (ulong)ThreadSafeRandom.Next();
            hiddenValue = Encrypt(value, currentCryptoKey);
#if DETECTOR
#if UNITY_EDITOR
            fakeValue = value;
            fakeValueActive = true;
#else
            var detectorRunning = ObscuredCheatingDetector.ExistsAndIsRunning;
            fakeValue = detectorRunning ? value : 0;
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
        public static ulong Encrypt(ulong value)
        {
            return Encrypt(value, 0);
        }

        /// <summary>
        /// 简单的对称加密，使用默认的加密密钥。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static ulong Encrypt(ulong value, ulong key)
        {
            if (key == 0)
            {
                return value ^ cryptoKey;
            }
            return value ^ key;
        }

        /// <summary>
        /// 解密
        /// </summary>
        public static ulong Decrypt(ulong value)
        {
            return Decrypt(value, 0);
        }

        /// <summary>
        /// 解密
        /// </summary>
        public static ulong Decrypt(ulong value, ulong key)
        {
            if (key == 0)
            {
                return value ^ cryptoKey;
            }
            return value ^ key;
        }

        private ulong InternalDecrypt()
        {
            if (!inited)
            {
                currentCryptoKey = (ulong)ThreadSafeRandom.Next();
                hiddenValue = Encrypt(0, currentCryptoKey);
#if DETECTOR
                fakeValue = 0;
                fakeValueActive = false;
#endif
                inited = true;

                return 0;
            }

            var decrypted = Decrypt(hiddenValue, currentCryptoKey);
#if DETECTOR
            if (ObscuredCheatingDetector.ExistsAndIsRunning && fakeValueActive && decrypted != fakeValue)
            {
                ObscuredCheatingDetector.Instance.OnCheatingDetected();
            }
#endif

            return decrypted;
        }

        #region overrides

        public static implicit operator TSULong(ulong value)
        {
            return new TSULong(value);
        }

        public static implicit operator ulong(TSULong value)
        {
            return value.InternalDecrypt();
        }

        public static TSULong operator ++(TSULong input)
        {
            var decrypted = input.InternalDecrypt() + 1L;
            input.hiddenValue = Encrypt(decrypted, input.currentCryptoKey);
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

        public static TSULong operator --(TSULong input)
        {
            var decrypted = input.InternalDecrypt() - 1L;
            input.hiddenValue = Encrypt(decrypted, input.currentCryptoKey);
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
        public static bool operator ==(TSULong left, TSULong right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(TSULong left, TSULong right)
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
            if (!(obj is TSULong))
                return false;
            return Equals((TSULong)obj);
        }

        public bool Equals(TSULong obj)
        {
            if (currentCryptoKey == obj.currentCryptoKey)
            {
                return hiddenValue == obj.hiddenValue;
            }

            return Decrypt(hiddenValue, currentCryptoKey) == Decrypt(obj.hiddenValue, obj.currentCryptoKey);
        }

        public int CompareTo(TSULong other)
        {
            return InternalDecrypt().CompareTo(other.InternalDecrypt());
        }

        public int CompareTo(ulong other)
        {
            return InternalDecrypt().CompareTo(other);
        }

        public int CompareTo(object obj)
        {
#if !NO_IL2CPP
            return InternalDecrypt().CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is ulong)) throw new ArgumentException("Argument must be ulong");
			return CompareTo((ulong)obj);
#endif
        }

        #endregion
    }
}

#endif