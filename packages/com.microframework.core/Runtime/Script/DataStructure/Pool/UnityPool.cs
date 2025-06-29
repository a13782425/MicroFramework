using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Core
{
    /// <summary>
    /// Unity的对象池
    /// <para>该对象池会创建一个空游戏物体作为根节点</para>
    /// </summary>
    public class UnityPool<T> where T : class
    {
        private static GameObject _poolObj = null;
        static UnityPool()
        {
            _poolObj = new GameObject("UnityPool");
            _poolObj.transform.SetParent(MicroContext.transform);
        }

        private GameObject _gameObject = null;
        public GameObject gameObject => _gameObject;

        private Transform _transform = null;
        public Transform transform => _transform;
        public string Name { get => _gameObject.name; set => _gameObject.name = value; }

        public event Func<T> onCreate;
        public event Action<T> onRecover;
        private MicroPool<T> _pool = null;
        public UnityPool()
        {
            _gameObject = new GameObject("UnityPool");
            _transform = _gameObject.transform;
            _transform.SetParent(_poolObj.transform);
            _pool = new MicroPool<T>(() =>
            {
                if (onCreate != null)
                    return onCreate();
                return null;
            }, (item) => { onRecover?.Invoke(item); });
        }
        public void Clear()
        {
            _pool.Clear();
        }

        public T Get()
        {
            return _pool.Get();
        }

        public bool IsEmpty() => _pool.IsEmpty();

        public void Recover(T t)
        {
            _pool.Recover(t);
        }
    }
}
