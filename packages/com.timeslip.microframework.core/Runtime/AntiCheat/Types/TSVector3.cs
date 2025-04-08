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

    public struct TSVector3
    {
        private static int cryptoKey = 120207;
        private static readonly Vector3 zero = Vector3.zero;


        private int currentCryptoKey;


        private EncryptedVector3 hiddenValue;


        private bool inited;
#if DETECTOR

        private Vector3 fakeValue;


        private bool fakeValueActive;
#endif

        private TSVector3(Vector3 value)
        {
            currentCryptoKey = ThreadSafeRandom.Next();
            hiddenValue = Encrypt(value, currentCryptoKey);
#if DETECTOR
#if UNITY_EDITOR
            fakeValue = value;
            fakeValueActive = true;
#else
            var detectorRunning = ObscuredCheatingDetector.ExistsAndIsRunning;
            fakeValue = detectorRunning ? value : zero;
            fakeValueActive = detectorRunning;
#endif
#endif
            inited = true;
        }

        /// <summary>
        /// Mimics constructor of regular Vector3.
        /// </summary>
        /// <param name="x">X component of the vector</param>
        /// <param name="y">Y component of the vector</param>
        /// <param name="z">Z component of the vector</param>
        public TSVector3(float x, float y, float z)
        {
            currentCryptoKey = ThreadSafeRandom.Next();
            hiddenValue = Encrypt(x, y, z, currentCryptoKey);
#if DETECTOR
            if (ObscuredCheatingDetector.ExistsAndIsRunning)
            {
                fakeValue = new Vector3(x, y, z);
                fakeValueActive = true;
            }
            else
            {
                fakeValue = zero;
                fakeValueActive = false;
            }
#endif
            inited = true;
        }

        public float x
        {
            get
            {
                var decrypted = InternalDecryptField(hiddenValue.x);
#if DETECTOR
                if (ObscuredCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue.x) > ObscuredCheatingDetector.Instance.Vector3Epsilon)
                {
                    ObscuredCheatingDetector.Instance.OnCheatingDetected();
                }
#endif
                return decrypted;
            }

            set
            {
                hiddenValue.x = InternalEncryptField(value);
#if DETECTOR
                if (ObscuredCheatingDetector.ExistsAndIsRunning)
                {
                    fakeValue.x = value;
                    fakeValue.y = InternalDecryptField(hiddenValue.y);
                    fakeValue.z = InternalDecryptField(hiddenValue.z);
                    fakeValueActive = true;
                }
                else
                {
                    fakeValueActive = false;
                }
#endif
            }
        }

        public float y
        {
            get
            {
                var decrypted = InternalDecryptField(hiddenValue.y);
#if DETECTOR
                if (ObscuredCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue.y) > ObscuredCheatingDetector.Instance.Vector3Epsilon)
                {
                    ObscuredCheatingDetector.Instance.OnCheatingDetected();
                }
#endif
                return decrypted;
            }

            set
            {
                hiddenValue.y = InternalEncryptField(value);
#if DETECTOR
                if (ObscuredCheatingDetector.ExistsAndIsRunning)
                {
                    fakeValue.x = InternalDecryptField(hiddenValue.x);
                    fakeValue.y = value;
                    fakeValue.z = InternalDecryptField(hiddenValue.z);
                    fakeValueActive = true;
                }
                else
                {
                    fakeValueActive = false;
                }
#endif
            }
        }

        public float z
        {
            get
            {
                var decrypted = InternalDecryptField(hiddenValue.z);
#if DETECTOR
                if (ObscuredCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue.z) > ObscuredCheatingDetector.Instance.Vector3Epsilon)
                {
                    ObscuredCheatingDetector.Instance.OnCheatingDetected();
                }
#endif
                return decrypted;
            }

            set
            {
                hiddenValue.z = InternalEncryptField(value);
#if DETECTOR
                if (ObscuredCheatingDetector.ExistsAndIsRunning)
                {
                    fakeValue.x = InternalDecryptField(hiddenValue.x);
                    fakeValue.y = InternalDecryptField(hiddenValue.y);
                    fakeValue.z = value;
                    fakeValueActive = true;
                }
                else
                {
                    fakeValueActive = false;
                }
#endif
            }
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    default:
                        throw new IndexOutOfRangeException("Invalid TSVector3 index!");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid TSVector3 index!");
                }
            }
        }

        /// <summary>
        /// 简单的对称加密，使用默认的加密密钥。
        /// </summary>
        public static EncryptedVector3 Encrypt(Vector3 value)
        {
            return Encrypt(value, 0);
        }

        /// <summary>
        /// 简单的对称加密，使用默认的加密密钥。
        /// </summary>
        public static EncryptedVector3 Encrypt(Vector3 value, int key)
        {
            return Encrypt(value.x, value.y, value.z, key);
        }

        /// <summary>
        /// 简单的对称加密，使用默认的加密密钥。
        /// </summary>
        public static EncryptedVector3 Encrypt(float x, float y, float z, int key)
        {
            if (key == 0)
            {
                key = cryptoKey;
            }

            EncryptedVector3 result;
            result.x = TSFloat.Encrypt(x, key);
            result.y = TSFloat.Encrypt(y, key);
            result.z = TSFloat.Encrypt(z, key);

            return result;
        }

        /// <summary>
        /// 解密
        /// </summary>
        public static Vector3 Decrypt(EncryptedVector3 value)
        {
            return Decrypt(value, 0);
        }

        /// <summary>
        /// 解密
        /// </summary>
        public static Vector3 Decrypt(EncryptedVector3 value, int key)
        {
            if (key == 0)
            {
                key = cryptoKey;
            }

            Vector3 result;
            result.x = TSFloat.Decrypt(value.x, key);
            result.y = TSFloat.Decrypt(value.y, key);
            result.z = TSFloat.Decrypt(value.z, key);

            return result;
        }

        private Vector3 InternalDecrypt()
        {
            if (!inited)
            {
                currentCryptoKey = ThreadSafeRandom.Next();
                hiddenValue = Encrypt(zero, currentCryptoKey);
#if DETECTOR
                fakeValue = zero;
                fakeValueActive = false;
#endif
                inited = true;

                return zero;
            }

            Vector3 value;

            value.x = TSFloat.Decrypt(hiddenValue.x, currentCryptoKey);
            value.y = TSFloat.Decrypt(hiddenValue.y, currentCryptoKey);
            value.z = TSFloat.Decrypt(hiddenValue.z, currentCryptoKey);
#if DETECTOR
            if (ObscuredCheatingDetector.ExistsAndIsRunning && fakeValueActive && !CompareVectorsWithTolerance(value, fakeValue))
            {
                ObscuredCheatingDetector.Instance.OnCheatingDetected();
            }
#endif

            return value;
        }
#if DETECTOR
        private bool CompareVectorsWithTolerance(Vector3 vector1, Vector3 vector2)
        {
            var epsilon = ObscuredCheatingDetector.Instance.Vector3Epsilon;
            return Math.Abs(vector1.x - vector2.x) < epsilon &&
                   Math.Abs(vector1.y - vector2.y) < epsilon &&
                   Math.Abs(vector1.z - vector2.z) < epsilon;
        }
#endif

        private float InternalDecryptField(int encrypted)
        {
            var key = cryptoKey;

            if (currentCryptoKey != cryptoKey)
            {
                key = currentCryptoKey;
            }

            var result = TSFloat.Decrypt(encrypted, key);
            return result;
        }

        private int InternalEncryptField(float encrypted)
        {
            var result = TSFloat.Encrypt(encrypted, cryptoKey);
            return result;
        }

        #region overrides

        public static implicit operator TSVector3(Vector3 value)
        {
            return new TSVector3(value);
        }

        public static implicit operator Vector3(TSVector3 value)
        {
            return value.InternalDecrypt();
        }

        public static TSVector3 operator +(TSVector3 a, TSVector3 b)
        {
            return a.InternalDecrypt() + b.InternalDecrypt();
        }

        public static TSVector3 operator +(Vector3 a, TSVector3 b)
        {
            return a + b.InternalDecrypt();
        }

        public static TSVector3 operator +(TSVector3 a, Vector3 b)
        {
            return a.InternalDecrypt() + b;
        }

        public static TSVector3 operator -(TSVector3 a, TSVector3 b)
        {
            return a.InternalDecrypt() - b.InternalDecrypt();
        }

        public static TSVector3 operator -(Vector3 a, TSVector3 b)
        {
            return a - b.InternalDecrypt();
        }

        public static TSVector3 operator -(TSVector3 a, Vector3 b)
        {
            return a.InternalDecrypt() - b;
        }

        public static TSVector3 operator -(TSVector3 a)
        {
            return -a.InternalDecrypt();
        }

        public static TSVector3 operator *(TSVector3 a, float d)
        {
            return a.InternalDecrypt() * d;
        }

        public static TSVector3 operator *(float d, TSVector3 a)
        {
            return d * a.InternalDecrypt();
        }

        public static TSVector3 operator /(TSVector3 a, float d)
        {
            return a.InternalDecrypt() / d;
        }

        public static bool operator ==(TSVector3 lhs, TSVector3 rhs)
        {
            return lhs.InternalDecrypt() == rhs.InternalDecrypt();
        }

        public static bool operator ==(Vector3 lhs, TSVector3 rhs)
        {
            return lhs == rhs.InternalDecrypt();
        }

        public static bool operator ==(TSVector3 lhs, Vector3 rhs)
        {
            return lhs.InternalDecrypt() == rhs;
        }

        public static bool operator !=(TSVector3 lhs, TSVector3 rhs)
        {
            return lhs.InternalDecrypt() != rhs.InternalDecrypt();
        }

        public static bool operator !=(Vector3 lhs, TSVector3 rhs)
        {
            return lhs != rhs.InternalDecrypt();
        }

        public static bool operator !=(TSVector3 lhs, Vector3 rhs)
        {
            return lhs.InternalDecrypt() != rhs;
        }

        public override bool Equals(object other)
        {
            return InternalDecrypt().Equals(other);
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

        #endregion


        public struct EncryptedVector3
        {
            public int x;

            public int y;

            public int z;
        }
    }
}

#endif