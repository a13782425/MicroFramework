using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFramework.Core
{
#if UNITY_EDITOR    
    public class EditorModuleMono : MonoBehaviour
    {
        public IMicroModule module { get; internal set; }
    }
#endif

    internal class ModuleDescribeLinkedNode
    {
        internal readonly ModuleDescribe module;
        internal ModuleDescribeLinkedNode nextNode;
        internal ModuleDescribeLinkedNode(ModuleDescribe module)
        {
            this.module = module;
        }
    }

    /// <summary>
    /// 模块的描述
    /// </summary>
    internal class ModuleDescribe
    {
        /// <summary>
        /// 模块接口类型
        /// </summary>
        private static readonly Type MODULE_INTERFACE_TYPE = typeof(IMicroModule);

        private GameObject _gameObject;

        /// <summary>
        /// 模块的游戏物体
        /// </summary>
        internal GameObject gameObject => _gameObject;
        /// <summary>
        /// 依赖的类型
        /// </summary>
        private Type[] _requireTypes;

        /// <summary>
        /// 依赖模块（需要哪些模块才能运行）
        /// </summary>
        private List<ModuleDescribe> _depDescribes = new List<ModuleDescribe>();

        /// <summary>
        /// 引用的模块（哪些模块用到了该模块）
        /// </summary>
        private List<ModuleDescribe> _usedDescribes = new List<ModuleDescribe>();
        /// <summary>
        /// 当前模块
        /// </summary>
        internal readonly IMicroModule Target;
        /// <summary>
        /// 模块名
        /// </summary>
        internal readonly string ModuleName;

        private ModuleState _state = ModuleState.None;
        /// <summary>
        /// 是否已经初始化
        /// </summary>
        internal ModuleState State

        {
            get { return _state; }
            private set
            {
                if (_state == value)
                    return;
                _state = value;
                switch (value)
                {
                    case ModuleState.None:
                        break;
                    case ModuleState.Initializing:
                        break;
                    case ModuleState.Running:
                        MicroContext.AddUpdate(Target as IMicroUpdate);
                        MicroContext.AddLogicUpdate(Target as IMicroLogicUpdate);
                        break;
                    case ModuleState.Suspended:
                        MicroContext.RemoveUpdate(Target as IMicroUpdate);
                        MicroContext.RemoveLogicUpdate(Target as IMicroLogicUpdate);
                        break;
                    case ModuleState.Destory:
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 获取所有依赖的类型
        /// </summary>
        /// <returns></returns>
        internal Type[] RequireTypes => _requireTypes;
        /// <summary>
        /// 依赖模块（需要哪些模块才能运行）
        /// </summary>
        /// <returns></returns>
        internal List<ModuleDescribe> DepDescribes => _depDescribes;
        /// <summary>
        /// 引用的模块（哪些模块用到了该模块）
        /// </summary>
        /// <returns></returns>
        internal List<ModuleDescribe> UsedDescribes => _usedDescribes;

        /// <summary>
        /// 拥有所有依赖
        /// </summary>
        internal bool HasAllDep { get; private set; }

        /// <summary>
        /// 模块是否准备完成
        /// </summary>
        public bool IsReady
        {
            get
            {
                if (!HasAllDep)
                {
                    m_updateDependency();
                    return false;
                }
                foreach (var item in _depDescribes)
                {
                    if (item.State != ModuleState.Running)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        internal ModuleDescribe(IMicroModule target)
        {
            this.Target = target;
            this.ModuleName = target.GetType().FullName;
            HasAllDep = false;
            RequireTypeAttribute attribute = target.GetType().GetCustomAttribute<RequireTypeAttribute>();
            if (attribute != null)
            {
                List<Type> tempList = new List<Type>();
                if (attribute.RequireTypes != null)
                {
                    foreach (var requireType in attribute.RequireTypes)
                    {
                        if (MODULE_INTERFACE_TYPE.IsAssignableFrom(requireType))
                        {
                            tempList.Add(requireType);
                        }
#if UNITY_EDITOR
                        else
                        {
                            MicroLogger.LogWarning("模块关联了一个不是模块的类型，类型名：" + requireType.FullName);
                        }
#endif
                    }
                }
                _requireTypes = tempList.ToArray();
            }
            else
            {
                _requireTypes = new Type[0];
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        internal void Init()
        {
            try
            {
                _gameObject = new GameObject(this.Target.GetType().Name);
#if UNITY_EDITOR
                var mono = _gameObject.AddComponent<EditorModuleMono>();
                mono.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave;
                mono.module = Target;
#endif
                State = ModuleState.Initializing;
                _gameObject.transform.SetParent(MicroContext.transform);
                Target.OnInit();
            }
            catch (Exception ex)
            {
                MicroLogger.LogError(Target.GetType().Name + "=>" + ex.Message);
            }
        }
        /// <summary>
        /// 销毁
        /// </summary>
        internal void Destroy()
        {
            try
            {
                if (gameObject != null)
                    Object.Destroy(gameObject);
                Target.OnDestroy();
                State = ModuleState.Destory;
            }
            catch (Exception ex)
            {
                MicroLogger.LogError(Target.GetType().Name + "=>" + ex.Message);
            }
        }

        /// <summary>
        /// 更新依赖
        /// </summary>
        private void m_updateDependency()
        {
            bool temp = true;
            foreach (Type dependType in _requireTypes)
            {
                ModuleDescribe moduleDescibe = MicroContext.GetModuleDescribe(dependType.FullName);
                if (moduleDescibe == null)
                {
                    temp = false;
                    break;
                }
                else
                {
                    AddDependency(moduleDescibe);
                }
            }
            HasAllDep = temp;
        }

        /// <summary>
        /// 移除依赖
        /// </summary>
        internal void DelDependency(ModuleDescribe moduleDescribe)
        {
            if (this._depDescribes.Contains(moduleDescribe))
            {
                this._depDescribes.Remove(moduleDescribe);
                moduleDescribe._usedDescribes.Remove(this);
                HasAllDep = false;
            }
        }

        /// <summary>
        /// 添加依赖
        /// </summary>
        private void AddDependency(ModuleDescribe moduleDescribe)
        {
            if (!this._depDescribes.Contains(moduleDescribe))
            {
                this._depDescribes.Add(moduleDescribe);
                moduleDescribe._usedDescribes.Add(this);
            }
        }
        internal void CheckInit()
        {
            if (Target.IsInit)
                State = ModuleState.Running;
        }
        /// <summary>
        /// 暂停模块
        /// </summary>
        internal void OnSuspend()
        {
            Target.OnSuspend();
            State = ModuleState.Suspended;

        }
        /// <summary>
        /// 重启模块
        /// </summary>
        internal void OnResume()
        {
            Target.OnResume();
            State = ModuleState.Running;
        }
    }
}
