using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.Core
{
    /// <summary>
    /// 更新对象的描述
    /// </summary>
    internal interface IUpdateDescribe
    {
        event Func<bool> onCheckCanUpdateEvent;
        IUpdateDescribe parent { get; set; }
        IUpdateDescribe child { get; set; }
        bool CanUpdate { get; }
        void OnExecute(float deltaTime);
        void Release();
    }

    internal class MicroUpdateDescribe : IUpdateDescribe
    {
        public IMicroUpdate microUpdate;
        private Func<bool> _checkCanUpdateEvent;
        public event Func<bool> onCheckCanUpdateEvent
        {
            add
            {
                if (value != null)
                    _checkCanUpdateEvent += value;
                _canUpdate = _checkCanUpdateEvent == null;
            }
            remove
            {
                if (value != null)
                    _checkCanUpdateEvent -= value;
                _canUpdate = _checkCanUpdateEvent == null;
            }
        }


        private bool _canUpdate = true;

        public bool CanUpdate => _canUpdate ? true : _checkCanUpdateEvent();

        public IUpdateDescribe parent { get; set; }
        public IUpdateDescribe child { get; set; }

        public void OnExecute(float deltaTime)
        {
            microUpdate.OnUpdate(deltaTime);
        }
        public void Release()
        {
            parent = null;
            child = null;
            microUpdate = null;
            _canUpdate = true;
            _checkCanUpdateEvent = null;
        }
        public override string ToString()
        {
            return this.microUpdate.GetType().FullName;
        }
    }

    internal class MicroLogicUpdateDescribe : IUpdateDescribe
    {
        public IMicroLogicUpdate microUpdate;
        private Func<bool> _checkCanUpdateEvent;
        public event Func<bool> onCheckCanUpdateEvent
        {
            add
            {
                if (value != null)
                    _checkCanUpdateEvent += value;
                _canUpdate = _checkCanUpdateEvent == null;
            }
            remove
            {
                if (value != null)
                    _checkCanUpdateEvent -= value;
                _canUpdate = _checkCanUpdateEvent == null;
            }
        }
        public IUpdateDescribe parent { get; set; }
        public IUpdateDescribe child { get; set; }
        private bool _canUpdate = false;

        private int _logicFrame = 0;
        private float _logicFrameTime = 0;
        public float logicFrameTime
        {
            get
            {
                if (_logicFrame != microUpdate.LogicFrame)
                {
                    _logicFrameTime = 1f / microUpdate.LogicFrame;
                    _logicFrame = microUpdate.LogicFrame;
                }
                return _logicFrameTime;
            }
        }

        private float _tempTime = 0;

        public bool CanUpdate => _canUpdate ? true : _checkCanUpdateEvent();

        public void OnExecute(float deltaTime)
        {
            _tempTime += deltaTime;
            while (_tempTime - logicFrameTime > 0)
            {
                _tempTime -= logicFrameTime;
                microUpdate.OnLogicUpdate(logicFrameTime);
            }
        }
        public void Release()
        {
            parent = null;
            child = null;
            microUpdate = null;
            _canUpdate = true;
            _checkCanUpdateEvent = null;
        }

        public override string ToString()
        {
            return this.microUpdate.GetType().FullName;
        }
    }
}
