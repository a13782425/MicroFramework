using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Core
{
    /// <summary>
    /// 继承mono的单例基类,所有单例切场景均布释放
    /// 默认情况下, Awake自动适配，应用退出时释放。
    /// Awake与OnDestory是可覆写函数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T instance = null;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject(typeof(T).ToString(), typeof(T)).GetComponent<T>();
                    instance.transform.SetParent(MicroContext.transform);
                }
                return instance;
            }
        }
    }
}
