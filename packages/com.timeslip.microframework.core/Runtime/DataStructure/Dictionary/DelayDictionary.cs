using System.Collections;
using System.Collections.Generic;

namespace MFramework.Core
{

    /// <summary>
    /// 延时字典
    /// 增加,删除,清空时候不会真正删除数据，需要执行Push方法才会更改原始数据
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DelayDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, TValue> _dic = new Dictionary<TKey, TValue>();
        private HashSet<TKey> _waitRemoveSet = new HashSet<TKey>();
        private Dictionary<TKey, TValue> _waitAddDic = new Dictionary<TKey, TValue>();
        /// <summary>
        /// 是否有脏数据
        /// </summary>
        private bool _isDirty = false;
        /// <summary>
        /// 是否清空
        /// </summary>
        private bool _isClear = false;
        public int Count => _dic.Count + _waitAddDic.Count - _waitRemoveSet.Count;
        /// <summary>
        /// 获取一个数据（在缓存中的数据无法通过该方法获取）
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get
            {
                TryGetValue(key, out TValue value);
                return value;
            }
        }

        /// <summary>
        /// 尝试获取一个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            if (_waitRemoveSet.Contains(key))
                return false;
            if (_dic.TryGetValue(key, out value))
                return true;
            if (_waitAddDic.TryGetValue(key, out value))
                return true;
            return false;
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            if (_waitRemoveSet.Contains(key))
            {
                return false;
            }
            return _dic.ContainsKey(key) || _waitAddDic.ContainsKey(key);
        }

        /// <summary>
        /// 删除一个数据
        /// </summary>
        /// <param name="key"></param>
        public void Remove(TKey key)
        {
            if (!_isDirty)
                PromiseYielder.ExecuteForNextEndOfFrame(Push);
            _isDirty = true;
            if (_waitAddDic.ContainsKey(key))
                _waitAddDic.Remove(key);
            else if (_dic.ContainsKey(key))
                _waitRemoveSet.Add(key);
        }

        /// <summary>
        /// 增加一个数据(如果增加Key相同的则更新数据)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            if (!_isDirty)
                PromiseYielder.ExecuteForNextEndOfFrame(Push);
            _isDirty = true;
            if (_waitRemoveSet.Contains(key))
                _waitRemoveSet.Remove(key);
            if (_waitAddDic.ContainsKey(key))
                _waitAddDic[key] = value;
            else
                _waitAddDic.Add(key, value);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            if (!_isDirty)
                PromiseYielder.ExecuteForNextEndOfFrame(Push);
            _isDirty = true;
            _isClear = true;
            _waitAddDic.Clear();
            _waitRemoveSet.Clear();
        }
        /// <summary>
        /// 将缓存数据推到正式数据
        /// </summary>
        public void Push()
        {
            if (!_isDirty) return;
            _isDirty = false;

            if (_isClear)
                _dic.Clear();
            if (_waitRemoveSet.Count > 0)
            {
                foreach (var item in _waitRemoveSet)
                {
                    _dic.Remove(item);
                }
                _waitRemoveSet.Clear();
            }
            if (_waitAddDic.Count > 0)
            {
                foreach (var item in _waitAddDic)
                {
                    _dic[item.Key] = item.Value;
                }
                _waitAddDic.Clear();
            }
        }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dic.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _dic.GetEnumerator();


    }
}
