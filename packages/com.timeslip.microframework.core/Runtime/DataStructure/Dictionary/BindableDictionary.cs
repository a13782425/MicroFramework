using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MFramework.Core
{
    /// <summary>
    /// 数据绑定字典
    /// </summary>
    [Ignore]
    public sealed class BindableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IBindable
    {
        private BindableHandle _classHandler = new BindableHandle();

        private Dictionary<TKey, BindableHandle<TValue>> _allHandlers = new Dictionary<TKey, BindableHandle<TValue>>();

        private Dictionary<TKey, TValue> _dic = new Dictionary<TKey, TValue>();
        private bool _childIsBindable = false;
        public TValue this[TKey key]
        {
            get => _dic[key];
            set
            {
                bool tryGet = TryGetValue(key, out TValue oldValue);
                _dic[key] = value;
                this.m_publish(key, oldValue, value, tryGet ? ContainerBindableState.Replace : ContainerBindableState.Add);
                if (_childIsBindable)
                {
                    if (oldValue is IBindable old)
                    {
                        old.SetParent(null);
                    }
                    if (value is IBindable @new)
                    {
                        @new.SetParent(this);
                    }
                }
            }
        }

        public ICollection<TKey> Keys => _dic.Keys;

        public ICollection<TValue> Values => _dic.Values;

        public int Count => _dic.Count;

        public bool IsReadOnly => false;
        public BindableDictionary()
        {
            _classHandler.Init(this, BindableUtils.BINDABLE_ALL_KEY, null);
            _childIsBindable = typeof(IBindable).IsAssignableFrom(typeof(TValue));
        }

        /// <summary>
        /// 等价于索引器的Set
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void RawSet(TKey key, TValue value)
        {
            TryGetValue(key, out TValue oldValue);
            _dic[key] = value;
            if (_childIsBindable)
            {
                if (oldValue is IBindable old)
                {
                    old.SetParent(null);
                }
                if (value is IBindable @new)
                {
                    @new.SetParent(this);
                }
            }
        }
        public void Add(TKey key, TValue value)
        {
            _dic.Add(key, value);
            TValue oldValue = default;
            this.m_publish(key, oldValue, value, ContainerBindableState.Add);
            if (_childIsBindable)
            {
                if (value is IBindable bindable)
                {
                    bindable.SetParent(this);
                }
            }
        }
        public void RawAdd(TKey key, TValue value)
        {
            _dic.Add(key, value);
            if (_childIsBindable)
            {
                if (value is IBindable bindable)
                {
                    bindable.SetParent(this);
                }
            }
        }
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void RawAdd(KeyValuePair<TKey, TValue> item)
        {
            RawAdd(item.Key, item.Value);
        }
        public void Clear()
        {
            if (_childIsBindable)
            {
                foreach (var item in _dic)
                {
                    if (item is IBindable bindable)
                    {
                        bindable.SetParent(null);
                    }
                }
            }
            _dic.Clear();
            _classHandler.Publish(new ContainerBindableEventArgs(this, BindableUtils.BINDABLE_ALL_KEY, ContainerBindableState.Clear), true);
        }
        public void RawClear()
        {
            if (_childIsBindable)
            {
                foreach (var item in _dic)
                {
                    if (item is IBindable bindable)
                    {
                        bindable.SetParent(null);
                    }
                }
            }
            _dic.Clear();
        }
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dic).Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return _dic.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dic).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dic.GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            _dic.TryGetValue(key, out TValue value);
            bool result = _dic.Remove(key);
            if (result)
            {
                this.m_publish(key, value, default, ContainerBindableState.Remove);
                if (_childIsBindable)
                {
                    if (value is IBindable bindable)
                    {
                        bindable.SetParent(null);
                    }
                }
            }
            return result;
        }
        public bool RawRemove(TKey key)
        {
            _dic.TryGetValue(key, out TValue value);
            bool result = _dic.Remove(key);
            if (result)
            {
                if (_childIsBindable)
                {
                    if (value is IBindable bindable)
                    {
                        bindable.SetParent(null);
                    }
                }
            }
            return result;
        }
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.Remove(item.Key);
        }
        public bool RawRemove(KeyValuePair<TKey, TValue> item)
        {
            return this.RawRemove(item.Key);
        }
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dic.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dic.GetEnumerator();
        }

        public void Subscribe(TKey key, BindableDelegate<TValue> callback, bool firstNotify = false, bool notifyAnyway = false)
        {
            m_getOrCreateHandle(key).Subscribe(callback, firstNotify, notifyAnyway);
        }

        public void Unsubscribe(TKey key, BindableDelegate<TValue> callback)
        {
            m_getOrCreateHandle(key).Unsubscribe(callback);
        }

        private void m_publish(TKey key, TValue oldValue, TValue newValue, ContainerBindableState state)
        {
            ContainerBindableEventArgs<TValue> args = new ContainerBindableEventArgs<TValue>(this, key, oldValue, newValue, state);
            (m_getHandle(key) ?? _classHandler).Publish(args, true);
        }

        private BindableHandle<TValue> m_getHandle(TKey key)
        {
            if (_allHandlers.TryGetValue(key, out BindableHandle<TValue> handle))
            {
                return handle;
            }
            return null;
        }
        private BindableHandle<TValue> m_getOrCreateHandle(TKey key)
        {
            if (!_allHandlers.TryGetValue(key, out BindableHandle<TValue> handle))
            {
                handle = new BindableHandle<TValue>();
                handle.Init(this, key, _classHandler, () =>
                {
                    if (_dic.TryGetValue(key, out TValue value))
                    {
                        return value;
                    }

                    return default(TValue);
                }, (v) => _dic[key] = v);
                _allHandlers[key] = handle;
            }
            return handle;
        }
        // 隐式转换
        public static implicit operator Dictionary<TKey, TValue>(BindableDictionary<TKey, TValue> bindDic)
        {
            return bindDic._dic;
        }

        // 显式转换
        public static explicit operator BindableDictionary<TKey, TValue>(Dictionary<TKey, TValue> dic)
        {
            BindableDictionary<TKey, TValue> bindDic = new BindableDictionary<TKey, TValue>();
            bindDic._dic = dic;
            return bindDic;
        }


        #region IBindable实现
        void IBindable.SetParent(IBindable parent)
        {
            if (parent == null)
            {
                _classHandler.SetParent(null);
                return;
            }
            _classHandler.SetParent(parent.GetHandle());
        }
        IBindableObserver IBindable.Subscribe(BindableDelegate callback, bool firstNotify, bool notifyAnyway)
        {
            return _classHandler.Subscribe(callback, firstNotify, notifyAnyway);
        }
        void IBindable.Unsubscribe(BindableDelegate callback)
        {
            _classHandler.Unsubscribe(callback);
        }
        void IBindable.UnsubscribeAll()
        {
            _classHandler.Clear();
            foreach (var item in _allHandlers)
            {
                item.Value.Clear();
            }
        }
        void IBindable.Publish()
        {
            _classHandler.Publish();
        }
        void IBindable.PublishAll()
        {
            foreach (var item in _allHandlers)
            {
                item.Value.Publish();
            }
        }
        void IBindable.SetNotify(bool notify, bool isPublish)
        {
            _classHandler.SetNotify(notify, isPublish);
            if (_childIsBindable)
            {
                foreach (var item in _dic)
                {
                    if (item.Value is IBindable bindable)
                    {
                        bindable.SetNotify(notify, isPublish);
                    }
                }
            }
            foreach (var item in _allHandlers)
            {
                item.Value.SetNotify(notify, isPublish);
            }
        }
        BindableHandle IBindable.GetHandle(object memberName = null)
        {
            if (memberName is TKey key)
                return m_getHandle(key);
            return _classHandler;
        }
        List<BindableHandle> IBindable.GetHandles()
        {
            return _allHandlers.Values.OfType<BindableHandle>().ToList();
        }
        #endregion
    }
}
