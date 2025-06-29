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
    
    public struct TSVector2Int
    {
        private static int cryptoKey = 160122;
        private static readonly Vector2Int zero = Vector2Int.zero;


        
        private int currentCryptoKey;

        
        private EncryptedVector2Int hiddenValue;

        
        private bool inited;
#if DETECTOR
        
        private Vector2Int fakeValue;

        
        private bool fakeValueActive;
#endif

        private TSVector2Int(Vector2Int value)
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

        public TSVector2Int(int x, int y)
        {
            currentCryptoKey = ThreadSafeRandom.Next();
            hiddenValue = Encrypt(x, y, currentCryptoKey);
#if DETECTOR
            if (ObscuredCheatingDetector.ExistsAndIsRunning)
            {
                fakeValue = new Vector2Int(x, y);
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

        public int x
        {
            get
            {
                var decrypted = InternalDecryptField(hiddenValue.x);
#if DETECTOR
                if (ObscuredCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue.x) > 0)
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
                    fakeValueActive = true;
                }
                else
                {
                    fakeValueActive = false;
                }
#endif
            }
        }

        public int y
        {
            get
            {
                var decrypted = InternalDecryptField(hiddenValue.y);
#if DETECTOR
                if (ObscuredCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue.y) > 0)
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
                    fakeValueActive = true;
                }
                else
                {
                    fakeValueActive = false;
                }
#endif
            }
        }

        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    default:
                        throw new IndexOutOfRangeException("Invalid TSVector2Int index!");
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
                    default:
                        throw new IndexOutOfRangeException("Invalid TSVector2Int index!");
                }
            }
        }

        /// <summary>
        /// 简单的对称加密，使用默认的加密密钥。
        /// </summary>
        public static EncryptedVector2Int Encrypt(Vector2Int value)
        {
            return Encrypt(value, 0);
        }

        /// <summary>
        /// 简单的对称加密，使用默认的加密密钥。
        /// </summary>
        public static EncryptedVector2Int Encrypt(Vector2Int value, int key)
        {
            return Encrypt(value.x, value.y, key);
        }

        /// <summary>
        /// 简单的对称加密，使用默认的加密密钥。
        /// </summary>
        public static EncryptedVector2Int Encrypt(int x, int y, int key)
        {
            if (key == 0)
            {
                key = cryptoKey;
            }

            EncryptedVector2Int result;
            result.x = TSInt.Encrypt(x, key);
            result.y = TSInt.Encrypt(y, key);

            return result;
        }

        /// <summary>
        /// 解密
        /// </summary>
        public static Vector2Int Decrypt(EncryptedVector2Int value)
        {
            return Decrypt(value, 0);
        }

        /// <summary>
        /// 解密
        /// </summary>
        public static Vector2Int Decrypt(EncryptedVector2Int value, int key)
        {
            if (key == 0)
            {
                key = cryptoKey;
            }

            var result = new Vector2Int
            {
                x = TSInt.Decrypt(value.x, key),
                y = TSInt.Decrypt(value.y, key)
            };

            return result;
        }

        private Vector2Int InternalDecrypt()
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

            var value = new Vector2Int
            {
                x = TSInt.Decrypt(hiddenValue.x, currentCryptoKey),
                y = TSInt.Decrypt(hiddenValue.y, currentCryptoKey)
            };
#if DETECTOR
            if (ObscuredCheatingDetector.ExistsAndIsRunning && fakeValueActive && value != fakeValue)
            {
                ObscuredCheatingDetector.Instance.OnCheatingDetected();
            }
#endif
            return value;
        }

        private int InternalDecryptField(int encrypted)
        {
            var key = cryptoKey;

            if (currentCryptoKey != cryptoKey)
            {
                key = currentCryptoKey;
            }

            var result = TSInt.Decrypt(encrypted, key);
            return result;
        }

        private int InternalEncryptField(int encrypted)
        {
            var result = TSInt.Encrypt(encrypted, cryptoKey);
            return result;
        }

        #region overrides

        public static implicit operator TSVector2Int(Vector2Int value)
        {
            return new TSVector2Int(value);
        }

        public static implicit operator Vector2Int(TSVector2Int value)
        {
            return value.InternalDecrypt();
        }

        public static implicit operator Vector2(TSVector2Int value)
        {
            return value.InternalDecrypt();
        }
        public override int GetHashCode()
        {
            return InternalDecrypt().GetHashCode();
        }

        public override string ToString()
        {
            return InternalDecrypt().ToString();
        }

        #endregion

        
        public struct EncryptedVector2Int
        {

            public int x;

            public int y;
        }
    }
}

#endif