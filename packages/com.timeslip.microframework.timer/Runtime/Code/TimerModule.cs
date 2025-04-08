using MFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Runtime
{
    /// <summary>
    /// 计时器
    /// </summary>
    public class TimerModule : IMicroModule, IMicroUpdate
    {

        private bool _isInit = false;
        public bool IsInit => _isInit;

        private Dictionary<int, Timer> _allTimerDic = new Dictionary<int, Timer>();

        /// <summary>
        /// 创建一个计时器
        /// </summary>
        /// <returns></returns>
        public Timer Create()
        {
            return Create(0, 1, null, null);
        }
        /// <summary>
        /// 创建一个计时器
        /// </summary>
        /// <returns></returns>
        public Timer Create(float delayTime)
        {
            return Create(delayTime, 1, null, null);
        }
        /// <summary>
        /// 创建一个计时器
        /// </summary>
        /// <returns></returns>
        public Timer Create(float delayTime, int loopCount)
        {
            return Create(delayTime, loopCount, null, null);
        }

        public Timer Create(float delayTime, Action<object> callback)
        {
            return Create(delayTime, 1, callback, null);
        }
        public Timer Create(float delayTime, Action<object> callback, object data)
        {
            return Create(delayTime, 1, callback, data);
        }
        public Timer Create(float delayTime, int loopCount, Action<object> callback)
        {
            return Create(delayTime, loopCount, callback, null);
        }
        public Timer Create(float delayTime, int loopCount, Action<object> callback, object data)
        {
            Timer timer = Timer.Create(this)
                               .SetDelayTime(delayTime)
                               .SetLoopCount(loopCount)
                               .SetCompleted(callback)
                               .SetUserData(data);
            _allTimerDic.Add(timer.GetHashCode(), timer);
            return timer;
        }

        /// <summary>
        /// 取消一个计时器
        /// </summary>
        /// <param name="timer"></param>
        internal void CancelTimer(Timer timer)
        {
            if (timer != null)
            {
                int hash = timer.GetHashCode();
                if (_allTimerDic.ContainsKey(hash))
                {
                    _allTimerDic.Remove(hash);
                }
            }
        }
        #region 生命周期
        public void OnDestroy()
        {
        }

        public void OnInit()
        {
            _isInit = true;
        }
        public void OnResume()
        {
        }

        public void OnSuspend()
        {
        }

        public void OnUpdate(float deltaTime)
        {
            Timer timer = null;
            try
            {
                List<int> keys = _allTimerDic.Keys.ToList();
                foreach (var item in keys)
                {
                    if (_allTimerDic.ContainsKey(item))
                    {
                        timer = _allTimerDic[item];
                        timer.Update(deltaTime);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.StackTrace);
                Debug.LogError(ex.Message);
                CancelTimer(timer);
            }
        }

        public void OnLogicUpdate(float deltaTime)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
