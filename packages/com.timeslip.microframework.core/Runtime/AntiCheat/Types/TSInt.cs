﻿#if ANTI_CHEAT
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
    public struct TSInt : IFormattable, IEquatable<TSInt>, IComparable<TSInt>, IComparable<int>, IComparable
    {
        private static int cryptoKey = 555555;


        private int currentCryptoKey;


        private int hiddenValue;


        private bool inited;

#if DETECTOR

        private int fakeValue;


        private bool fakeValueActive;
#endif

        private TSInt(int value)
        {
            currentCryptoKey = ThreadSafeRandom.Next();
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
        public static int Encrypt(int value)
        {
            return Encrypt(value, 0);
        }

        /// <summary>
        /// 简单的对称加密，使用默认的加密密钥。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int Encrypt(int value, int key)
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
        public static int Decrypt(int value)
        {
            return Decrypt(value, 0);
        }

        /// <summary>
        /// 解密
        /// </summary>
        public static int Decrypt(int value, int key)
        {
            if (key == 0)
            {
                return value ^ cryptoKey;
            }
            return value ^ key;
        }

        private int InternalDecrypt()
        {
            if (!inited)
            {
                currentCryptoKey = ThreadSafeRandom.Next();
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

        public static implicit operator TSInt(int value)
        {
            return new TSInt(value);
        }

        public static implicit operator int(TSInt value)
        {
            return value.InternalDecrypt();
        }

        public static implicit operator TSFloat(TSInt value)
        {
            return value.InternalDecrypt();
        }

        public static implicit operator TSDouble(TSInt value)
        {
            return value.InternalDecrypt();
        }

        public static explicit operator TSUInt(TSInt value)
        {
            return (uint)value.InternalDecrypt();
        }

        public static TSInt operator ++(TSInt input)
        {
            var decrypted = input.InternalDecrypt() + 1;
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

        public static TSInt operator --(TSInt input)
        {
            var decrypted = input.InternalDecrypt() - 1;
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
            if (!(obj is TSInt))
                return false;
            return Equals((TSInt)obj);
        }

        public bool Equals(TSInt obj)
        {
            if (currentCryptoKey == obj.currentCryptoKey)
            {
                return hiddenValue == obj.hiddenValue;
            }

            return Decrypt(hiddenValue, currentCryptoKey) == Decrypt(obj.hiddenValue, obj.currentCryptoKey);
        }

        public int CompareTo(TSInt other)
        {
            return InternalDecrypt().CompareTo(other.InternalDecrypt());
        }

        public int CompareTo(int other)
        {
            return InternalDecrypt().CompareTo(other);
        }

        public int CompareTo(object obj)
        {
#if !NO_IL2CPP
            return InternalDecrypt().CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is int)) throw new ArgumentException("Argument must be int");
			return CompareTo((int)obj);
#endif
        }
        public static bool operator ==(TSInt left, TSInt right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(TSInt left, TSInt right)
        {
            return !left.Equals(right);
        }
        #endregion
    }
}

#endif