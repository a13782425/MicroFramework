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

    public struct TSChar : IEquatable<TSChar>, IComparable<TSChar>, IComparable<char>, IComparable
    {
        private static char cryptoKey = '\x2014';

        private char currentCryptoKey;
        private bool inited;
        private char hiddenValue;
#if DETECTOR
        private char fakeValue;
        private bool fakeValueActive;
#endif
        private TSChar(char value)
        {
            currentCryptoKey = (char)ThreadSafeRandom.Next(char.MaxValue);
            hiddenValue = EncryptDecrypt(value, currentCryptoKey);
#if DETECTOR
#if UNITY_EDITOR
            fakeValue = value;
            fakeValueActive = true;
#else
            var detectorRunning = ObscuredCheatingDetector.ExistsAndIsRunning;
            fakeValue = detectorRunning ? value : '\0';
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
        public static char EncryptDecrypt(char value)
        {
            return EncryptDecrypt(value, '\0');
        }

        /// <summary>
        /// 简单的对称加密，使用默认的加密密钥。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static char EncryptDecrypt(char value, char key)
        {
            if (key == '\0')
            {
                return (char)(value ^ cryptoKey);
            }
            return (char)(value ^ key);
        }

        private char InternalDecrypt()
        {
            if (!inited)
            {
                currentCryptoKey = (char)ThreadSafeRandom.Next(char.MaxValue);
                hiddenValue = EncryptDecrypt('\0', currentCryptoKey);
#if DETECTOR
                fakeValue = '\0';
                fakeValueActive = false;
#endif
                inited = true;

                return '\0';
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

        public static implicit operator TSChar(char value)
        {
            return new TSChar(value);
        }

        public static implicit operator char(TSChar value)
        {
            return value.InternalDecrypt();
        }

        public static TSChar operator ++(TSChar input)
        {
            var decrypted = (char)(input.InternalDecrypt() + 1);
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

        public static TSChar operator --(TSChar input)
        {
            var decrypted = (char)(input.InternalDecrypt() - 1);
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
        public static bool operator ==(TSChar left, TSChar right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(TSChar left, TSChar right)
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
#if !UNITY_WINRT
        public string ToString(IFormatProvider provider)
        {
            return InternalDecrypt().ToString(provider);
        }
#endif

        public override bool Equals(object obj)
        {
            if (!(obj is TSChar))
                return false;
            return Equals((TSChar)obj);
        }
        public bool Equals(TSChar obj)
        {
            if (currentCryptoKey == obj.currentCryptoKey)
            {
                return hiddenValue == obj.hiddenValue;
            }

            return EncryptDecrypt(hiddenValue, currentCryptoKey) == EncryptDecrypt(obj.hiddenValue, obj.currentCryptoKey);
        }

        public int CompareTo(TSChar other)
        {
            return InternalDecrypt().CompareTo(other.InternalDecrypt());
        }

        public int CompareTo(char other)
        {
            return InternalDecrypt().CompareTo(other);
        }

        public int CompareTo(object obj)
        {
#if !NO_IL2CPP
            return InternalDecrypt().CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is char)) throw new ArgumentException("Argument must be char");
			return CompareTo((char)obj);
#endif
        }
        #endregion
    }
}

#endif