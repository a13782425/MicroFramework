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
using UnityEngine.Serialization;

namespace MFramework.Core
{
    public struct TSBool : IEquatable<TSBool>, IComparable<TSBool>, IComparable<bool>, IComparable
    {
        private static byte cryptoKey = 215;
        private byte currentCryptoKey;

        private bool inited;
        private int hiddenValue;
#if DETECTOR
        private bool fakeValue;

        private bool fakeValueActive;
#endif

        private TSBool(bool value)
        {
            currentCryptoKey = (byte)ThreadSafeRandom.Next(150);
            hiddenValue = Encrypt(value, currentCryptoKey);

#if DETECTOR
#if UNITY_EDITOR
            fakeValue = value;
            fakeValueActive = true;
#else
            var detectorRunning = ObscuredCheatingDetector.ExistsAndIsRunning;
            fakeValue = detectorRunning ? value : false;
            fakeValueActive = detectorRunning;
#endif
#endif

            inited = true;
        }

        /// <summary>
        /// 使用传递的密钥加密Bool值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int Encrypt(bool value, byte key)
        {
            if (key == 0)
            {
                key = cryptoKey;
            }

            var encryptedValue = value ? 213 : 181;

            encryptedValue ^= key;

            return encryptedValue;
        }

        /// <summary>
        /// 使用传递的密钥解密
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Decrypt(int value, byte key)
        {
            if (key == 0)
            {
                key = cryptoKey;
            }

            value ^= key;

            return value != 181;
        }

        private bool InternalDecrypt()
        {
            if (!inited)
            {
                currentCryptoKey = (byte)ThreadSafeRandom.Next(150);
                hiddenValue = Encrypt(false, currentCryptoKey);
#if DETECTOR
                fakeValue = false;
                fakeValueActive = false;
#endif
                inited = true;

                return false;
            }

            var value = hiddenValue;
            value ^= currentCryptoKey;

            var decrypted = value != 181;
#if DETECTOR
            if (ObscuredCheatingDetector.ExistsAndIsRunning && fakeValueActive && decrypted != fakeValue)
            {
                ObscuredCheatingDetector.Instance.OnCheatingDetected();
            }
#endif

            return decrypted;
        }

        #region overrides

        public static implicit operator TSBool(bool value)
        {
            return new TSBool(value);
        }

        public static implicit operator bool(TSBool value)
        {
            return value.InternalDecrypt();
        }
        public static bool operator ==(TSBool left, TSBool right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(TSBool left, TSBool right)
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
        public override bool Equals(object obj)
        {
            if (!(obj is TSBool))
                return false;
            return Equals((TSBool)obj);
        }

        public bool Equals(TSBool obj)
        {
            if (currentCryptoKey == obj.currentCryptoKey)
            {
                return hiddenValue == obj.hiddenValue;
            }

            return Decrypt(hiddenValue, currentCryptoKey) == Decrypt(obj.hiddenValue, obj.currentCryptoKey);
        }

        public int CompareTo(TSBool other)
        {
            return InternalDecrypt().CompareTo(other.InternalDecrypt());
        }

        public int CompareTo(bool other)
        {
            return InternalDecrypt().CompareTo(other);
        }

        public int CompareTo(object obj)
        {
#if !NO_IL2CPP
            return InternalDecrypt().CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is bool)) throw new ArgumentException("Argument must be boolean");
			return CompareTo((bool)obj);
#endif
        }
        #endregion

    }
}

#endif