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

    public struct TSSByte : IFormattable, IEquatable<TSSByte>, IComparable<TSSByte>, IComparable<sbyte>, IComparable
    {
        private static sbyte cryptoKey = 112;

        private sbyte currentCryptoKey;
        private sbyte hiddenValue;
        private bool inited;

#if DETECTOR
        private sbyte fakeValue;
        private bool fakeValueActive;
#endif

        private TSSByte(sbyte value)
        {
            currentCryptoKey = (sbyte)ThreadSafeRandom.Next(sbyte.MaxValue);
            hiddenValue = EncryptDecrypt(value, currentCryptoKey);
#if DETECTOR
#if UNITY_EDITOR
            fakeValue = value;
            fakeValueActive = true;
#else
            var detectorRunning = ObscuredCheatingDetector.ExistsAndIsRunning;
            fakeValue = detectorRunning ? value : (sbyte)0;
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
        public static sbyte EncryptDecrypt(sbyte value)
        {
            return EncryptDecrypt(value, 0);
        }

        /// <summary>
        /// 简单的对称加解密，使用默认的加密密钥。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static sbyte EncryptDecrypt(sbyte value, sbyte key)
        {
            if (key == 0)
            {
                return (sbyte)(value ^ cryptoKey);
            }
            return (sbyte)(value ^ key);
        }

        private sbyte InternalDecrypt()
        {
            if (!inited)
            {
                currentCryptoKey = (sbyte)ThreadSafeRandom.Next(sbyte.MaxValue);
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

        public static implicit operator TSSByte(sbyte value)
        {
            return new TSSByte(value);
        }

        public static implicit operator sbyte(TSSByte value)
        {
            return value.InternalDecrypt();
        }

        public static TSSByte operator ++(TSSByte input)
        {
            var decrypted = (sbyte)(input.InternalDecrypt() + 1);
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

        public static TSSByte operator --(TSSByte input)
        {
            var decrypted = (sbyte)(input.InternalDecrypt() - 1);
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
        public static bool operator ==(TSSByte left, TSSByte right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(TSSByte left, TSSByte right)
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
            if (!(obj is TSSByte))
                return false;
            return Equals((TSSByte)obj);
        }

        public bool Equals(TSSByte obj)
        {
            if (currentCryptoKey == obj.currentCryptoKey)
            {
                return hiddenValue == obj.hiddenValue;
            }

            return EncryptDecrypt(hiddenValue, currentCryptoKey) == EncryptDecrypt(obj.hiddenValue, obj.currentCryptoKey);
        }

        public int CompareTo(TSSByte other)
        {
            return InternalDecrypt().CompareTo(other.InternalDecrypt());
        }

        public int CompareTo(sbyte other)
        {
            return InternalDecrypt().CompareTo(other);
        }

        public int CompareTo(object obj)
        {
#if !NO_IL2CPP
            return InternalDecrypt().CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is sbyte)) throw new ArgumentException("Argument must be sbyte");
			return CompareTo((sbyte)obj);
#endif
        }
        #endregion
    }
}

#endif