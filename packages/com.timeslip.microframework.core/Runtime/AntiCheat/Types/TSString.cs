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
    
    public sealed class TSString : IComparable<TSString>, IComparable<string>, IComparable
    {
        private static string cryptoKey = "5556";

        
        private string currentCryptoKey;

        
        private byte[] hiddenValue;

        
        private bool inited;
#if DETECTOR
        
        private string fakeValue;

        
        private bool fakeValueActive;
#endif

        // for serialization purposes
        private TSString() { }

        private TSString(string value)
        {
            currentCryptoKey = ThreadSafeRandom.Next().ToString();
            hiddenValue = InternalEncrypt(value, currentCryptoKey);
#if DETECTOR
#if UNITY_EDITOR
            fakeValue = value;
            fakeValueActive = true;
#else
            var detectorRunning = ObscuredCheatingDetector.ExistsAndIsRunning;
            fakeValue = detectorRunning ? value : null;
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
        public static string EncryptDecrypt(string value)
        {
            return EncryptDecrypt(value, string.Empty);
        }

        /// <summary>
        /// 简单的对称加解密，使用默认的加密密钥。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EncryptDecrypt(string value, string key)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(key))
            {
                key = cryptoKey;
            }

            var keyLength = key.Length;
            var valueLength = value.Length;

            var result = new char[valueLength];

            for (var i = 0; i < valueLength; i++)
            {
                result[i] = (char)(value[i] ^ key[i % keyLength]);
            }

            return new string(result);
        }

        private static byte[] InternalEncrypt(string value)
        {
            return InternalEncrypt(value, cryptoKey);
        }

        private static byte[] InternalEncrypt(string value, string key)
        {
            return GetBytes(EncryptDecrypt(value, key));
        }

        private string InternalDecrypt()
        {
            if (!inited)
            {
                currentCryptoKey = ThreadSafeRandom.Next().ToString();
                hiddenValue = InternalEncrypt(string.Empty, currentCryptoKey);
#if DETECTOR
                fakeValue = string.Empty;
                fakeValueActive = false;
#endif
                inited = true;

                return string.Empty;
            }

            var key = currentCryptoKey;
            if (string.IsNullOrEmpty(key))
            {
                key = cryptoKey;
            }

            var result = EncryptDecrypt(GetString(hiddenValue), key);
#if DETECTOR
            if (ObscuredCheatingDetector.ExistsAndIsRunning && fakeValueActive && result != fakeValue)
            {
                ObscuredCheatingDetector.Instance.OnCheatingDetected();
            }
#endif

            return result;
        }

        #region overrides


        public int Length
        {
            get { return hiddenValue.Length / sizeof(char); }
        }

        public static implicit operator TSString(string value)
        {
            return value == null ? null : new TSString(value);
        }

        public static implicit operator string(TSString value)
        {
            return value == null ? null : value.InternalDecrypt();
        }

        public static bool operator ==(TSString a, TSString b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if ((object)a == null || (object)b == null)
            {
                return false;
            }

            if (a.currentCryptoKey == b.currentCryptoKey)
            {
                return ArraysEquals(a.hiddenValue, b.hiddenValue);
            }

            return string.Equals(a.InternalDecrypt(), b.InternalDecrypt());
        }

        public static bool operator !=(TSString a, TSString b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return InternalDecrypt().GetHashCode();
        }

        public override string ToString()
        {
            return InternalDecrypt();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TSString))
                return false;

            return Equals((TSString)obj);
        }

        public bool Equals(TSString value)
        {
            if (value == null) return false;

            if (currentCryptoKey == value.currentCryptoKey)
            {
                return ArraysEquals(hiddenValue, value.hiddenValue);
            }

            return string.Equals(InternalDecrypt(), value.InternalDecrypt());
        }

        public bool Equals(TSString value, StringComparison comparisonType)
        {
            if (value == null) return false;

            return string.Equals(InternalDecrypt(), value.InternalDecrypt(), comparisonType);
        }

        public int CompareTo(TSString other)
        {
            return InternalDecrypt().CompareTo(other.InternalDecrypt());
        }

        public int CompareTo(string other)
        {
            return InternalDecrypt().CompareTo(other);
        }

        public int CompareTo(object obj)
        {
#if !NO_IL2CPP
            return InternalDecrypt().CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is string)) throw new ArgumentException("Argument must be string");
			return CompareTo((string)obj);
#endif
        }

        #endregion

        private static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static string GetString(byte[] bytes)
        {
            var chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private static bool ArraysEquals(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
            {
                return true;
            }

            if ((a1 != null) && (a2 != null))
            {
                if (a1.Length != a2.Length)
                {
                    return false;
                }
                for (var i = 0; i < a1.Length; i++)
                {
                    if (a1[i] != a2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}

#endif