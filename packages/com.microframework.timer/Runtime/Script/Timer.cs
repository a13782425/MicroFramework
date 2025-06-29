using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.Runtime
{
    /// <summary>
    /// 计时器
    /// </summary>
    public sealed class Timer
    {
        private object _userData = null;
        private Action<object> _onCompleted = null;
        private float _delayTime = 0;
        private int _loopCount = 1;
        private bool _isStart = false;
        private bool _isPause = false;
        private float _interval = 0f;
        private static TimerModule _timeModule;

        private Timer()
        {
            _loopCount = 1;
        }
        /// <summary>
        /// 创建一个计时器
        /// </summary>
        /// <returns></returns>
        internal static Timer Create(TimerModule timerModule)
        {
            var timer = new Timer();
            _timeModule = timerModule;
            return timer;
        }
        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Timer SetUserData(object data)
        {
            _userData = data;
            return this;
        }
        /// <summary>
        /// 设置回调
        /// </summary>
        /// <param name="completed"></param>
        /// <returns></returns>
        public Timer SetCompleted(Action<object> completed)
        {
            _onCompleted = completed;
            return this;
        }
        /// <summary>
        /// 设置延迟时间
        /// </summary>
        /// <returns></returns>
        public Timer SetDelayTime(float second)
        {
            _delayTime = second;
            return this;
        }
        /// <summary>
        /// 设置循环次数
        /// </summary>
        /// <param name="count">-1为无限循环</param>
        /// <returns></returns>
        public Timer SetLoopCount(int count)
        {
            _loopCount = count;
            return this;
        }
        /// <summary>
        /// 取消一个计时器
        /// </summary>
        /// <returns></returns>
        public Timer Cancel()
        {
            _timeModule.CancelTimer(this);
            return this;
        }
        /// <summary>
        /// 开始计时器
        /// </summary>
        /// <returns></returns>
        public Timer Start()
        {
            _interval = 0;
            _isStart = true;
            return this;
        }
        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="pause"></param>
        /// <returns></returns>
        public Timer Pause(bool pause)
        {
            _isPause = pause;
            return this;
        }
        internal void Update(float deltaTime)
        {
            if (_isStart)
            {
                if (!_isPause)
                {
                    _interval += deltaTime;
                    if (_interval >= _delayTime)
                    {
                        if (_loopCount != 0)
                        {
                            _onCompleted?.Invoke(_userData);
                            int tempCount = _loopCount - 1;
                            if (_loopCount < 0 || tempCount > 0)
                            {
                                _loopCount = tempCount < 0 ? -1 : tempCount;
                                _interval = 0;
                            }
                            else
                            {
                                Cancel();
                            }
                        }
                        else
                        {
                            Cancel();
                        }
                    }
                }
            }
        }
    }
}
