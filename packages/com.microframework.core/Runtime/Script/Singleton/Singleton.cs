using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Core
{
    /// <summary>
    /// 通用继承形单例    
    /// </summary>
    /// <typeparam name="T">继承自此单例的可构造类型</typeparam>
    public abstract class Singleton<T> : IDisposable where T : class, new()
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }
        /// <summary>
        /// 非空虚方法，IDispose接口
        /// </summary>
        public virtual void Dispose()
        {
            instance = null;
        }
    }
}
