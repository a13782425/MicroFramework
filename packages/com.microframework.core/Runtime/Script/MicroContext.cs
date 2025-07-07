using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace MFramework.Core
{
    /// <summary>
    /// 更新的委托
    /// </summary>
    /// <param name="deltaTime"></param>
    public delegate void UpdateDelegate(float deltaTime);

    /// <summary>
    /// 无参委托
    /// </summary>
    public delegate void VoidDelegate();
    /// <summary>
    /// 微框架上下文
    /// </summary>
    public static partial class MicroContext
    {
        /// <summary>
        /// 模块接口类型
        /// </summary>
        internal static readonly Type MODULE_INTERFACE_TYPE = typeof(IMicroModule);

        [ThreadStatic]
        private static List<string> aliasNameList = default;
        #region prop

        /// <summary>
        /// 更新委托
        /// </summary>
        public static event UpdateDelegate onUpdate;
        /// <summary>
        /// 逻辑更新委托
        /// 以MicroSetting.logicDeltaTime为准
        /// </summary>
        public static event UpdateDelegate onLogicUpdate;
        /// <summary>
        /// 模块准备完成时调用
        /// <para>如果中途增加或删除模块，该回调会再次调用</para>
        /// </summary>
        public static event VoidDelegate onModuleReady
        {
            add
            {
                _onModuleReady += value;
                if (_isModuleReady)
                    value.Invoke();
            }
            remove
            {
                _onModuleReady -= value;
            }
        }
        /// <summary>
        /// 上下文关联的游戏物体
        /// </summary>
        public static GameObject gameObject { get; private set; }
        /// <summary>
        /// 上下文关联的Transform
        /// </summary>
        public static Transform transform { get; private set; }
        /// <summary>
        /// 微框架上下文日志
        /// </summary>
        public static IMicroLogger logger { get; private set; }
        /// <summary>
        /// 主线程上下文
        /// </summary>
        public static SynchronizationContext mainSynchronizationContext { get => _mainSynchronizationContext; set => _mainSynchronizationContext = value; }
        /// <summary>
        /// 微框架是否已经启动
        /// </summary>
        public static bool IsLaunch => _isLaunch;
        #endregion

        #region field
        /// <summary>
        ///  用于线程同步；多线程的数据发送到unity的主线程中；
        /// </summary>
        private static SynchronizationContext _mainSynchronizationContext;
        /// <summary>
        /// Unity的线程Id
        /// </summary>
        public static readonly int MainThreadId;
        /// <summary>
        /// 上下文Mono
        /// </summary>
        private static GameContextMono _gameMono;
        /// <summary>
        /// 模块准备完成时调用
        /// <para>如果中途增加或删除模块，该回调会再次调用</para>
        /// </summary>
        private static VoidDelegate _onModuleReady;
        /// <summary>
        /// 逻辑更新记录时间
        /// </summary>
        private static float _logicUpdateRecordTime = 0;
        /// <summary>
        /// 每帧时间
        /// </summary>
        private static float _updateDeltaTime = 0;
        /// <summary>
        /// 模块是否准备完毕
        /// </summary>
        private static bool _isModuleReady = false;
        /// <summary>
        /// 是否已经启动
        /// </summary>
        private static bool _isLaunch = false;

        #endregion

        #region public

        static MicroContext()
        {
            mainSynchronizationContext = SynchronizationContext.Current;
            MainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// 初始化框架
        /// 默认去Resources下面加载MicroRuntimeConfig
        /// </summary>
        public static void Launch()
        {
            if (_isLaunch)
                return;
            Launch(Resources.Load<MicroRuntimeConfig>("MicroRuntimeConfig"));
        }
        /// <summary>
        /// 初始化框架
        /// 自定义MicroRuntimeConfig
        /// 如果等于参数是null，则只会启动最基础的框架（没有任何模块的）
        /// </summary>
        public static void Launch(MicroRuntimeConfig runtimeConfig)
        {
            if (_isLaunch)
                return;
            _isLaunch = true;
            gameObject = new GameObject("GameContext");
            transform = gameObject.transform;
            _gameMono = gameObject.AddComponent<GameContextMono>();
            _gameMono.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            GameObject.DontDestroyOnLoad(gameObject);
            logger = MicroLogger.GetMicroLogger("MicroContext");
            if (runtimeConfig == null)
            {
                logger.LogError("运行时配置为空，启动失败");
                return;
            }
            MicroRuntimeConfig.CurrentConfig = runtimeConfig;

            foreach (var item in runtimeConfig.InitModules)
            {
                Type moduleType = item.CurrentType;
                if (moduleType == null)
                {
                    logger.LogWarning($"模块类型:{item.TypeName} 没有找到");
                    continue;
                }
                _isModuleReady = false;
                //执行模块
                IMicroModule module = Activator.CreateInstance(moduleType) as IMicroModule;
                logger.Log($"游戏模块:{moduleType.Name},注册成功");
                m_createModuleDescribe(module);
            }
        }

        /// <summary>
        /// 注册一个模块
        /// 同时也会将其实现IModule的接口注册进去
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T RegisterModule<T>() where T : class, IMicroModule, new()
        {
            string moduleName = typeof(T).FullName;
            if (InternalMicroData.moduleAlias.ContainsKey(moduleName))
            {
                logger.LogError($"当前模块已存在，模块名：{moduleName}");
                return InternalMicroData.moduleAlias[moduleName].module.Target as T;
            }
            _isModuleReady = false;
            T t = new T();
            m_createModuleDescribe(t);
            return t;
        }

        /// <summary>
        /// 移除一个模块,只能传入最顶层的类型
        /// 如果移除的模块是其他模块的依赖项，则其他依赖模块也会停止运作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool RemoveModule<T>() where T : class, IMicroModule, new()
        {
            Type rmType = typeof(T);
            ModuleAliasDescribe linkedNode = m_getModuleDescribeLinkedNode(rmType.Name);
            ModuleDescribe moduleDescribe = null;
            while (linkedNode != null)
            {
                if (linkedNode.module.Target.GetType() == rmType)
                {
                    moduleDescribe = linkedNode.module;
                    break;
                }
                linkedNode = linkedNode.child;
            }
            if (moduleDescribe == null)
                throw new ArgumentException($"当前模块不存在，请先注册，模块名：{rmType.Name}");
            try
            {
                moduleDescribe.Destroy();
                string[] aliasNames = m_getModuleAliasNames(rmType);
                m_suspendModuleDependency(moduleDescribe);
                //替换模块
                foreach (var item in aliasNames)
                {
                    if (!InternalMicroData.moduleAlias.ContainsKey(item))
                        continue;
                    linkedNode = InternalMicroData.moduleAlias[item];
                    if (linkedNode.module == moduleDescribe && linkedNode.child == null)
                        InternalMicroData.moduleAlias.Remove(item);
                    else if (linkedNode.module == moduleDescribe && linkedNode.child != null)
                        InternalMicroData.moduleAlias.Add(item, linkedNode.child);
                    else if (linkedNode.module != moduleDescribe)
                    {
                        ModuleAliasDescribe nextNode = linkedNode.child;
                        while (nextNode != null)
                        {
                            if (nextNode.module == moduleDescribe)
                            {
                                linkedNode.child = nextNode.child;
                                break;
                            }
                            else
                            {
                                linkedNode = nextNode;
                                nextNode = nextNode.child;
                            }
                        }
                    }
                }
                m_onlyRemoveModule(moduleDescribe);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"模块删除错误，模块名：{rmType.Name}");
                throw ex;
            }

        }

        /// <summary>
        /// 获取一个模块，可以使用注册时候的类型，或者接口获取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T GetModule<T>() where T : class, IMicroModule
        {
            string moduleName = typeof(T).FullName;
            ModuleDescribe describe = GetModuleDescribe(moduleName);
            if (describe == null)
            {
                logger.LogWarning($"当前模块不存在，请先注册，模块名：{moduleName}");
                return null;
            }
            return describe.Target as T;
        }

        /// <summary>
        /// 添加一个更新
        /// </summary>
        /// <param name="microUpdate">更新对象</param>
        /// <param name="onCheckCanUpdate">检测当前对象是否可以更新,如果不提供默认可以更新</param>
        /// <returns></returns>
        public static IMicroUpdate AddUpdate(IMicroUpdate microUpdate, Func<bool> onCheckCanUpdate = null)
        {
            if (microUpdate == null)
                return microUpdate;
            // 检查是否已经存在于更新列表中
            if (InternalMicroData.cacheUpdateDict.TryGetValue(microUpdate, out IUpdateDescribe existDescribe))
            {
                // 如果已存在，更新检查事件并返回
                existDescribe.onCheckCanUpdateEvent += onCheckCanUpdate;
                return microUpdate;
            }

            MicroUpdateDescribe describe = InternalMicroData.updatePool.Get();
            describe.microUpdate = microUpdate;
            describe.onCheckCanUpdateEvent += onCheckCanUpdate;
            // 添加到缓存字典
            InternalMicroData.cacheUpdateDict.Add(microUpdate, describe);
            if (InternalMicroData.updateDescribe == null)
            {
                InternalMicroData.updateDescribe = describe;
                InternalMicroData.lastUpdateDescribe = describe;
            }
            else
            {
                InternalMicroData.lastUpdateDescribe.child = describe;
                describe.parent = InternalMicroData.lastUpdateDescribe;
                InternalMicroData.lastUpdateDescribe = describe;
            }
            return microUpdate;
        }
        /// <summary>
        /// 移除一个更新
        /// </summary>
        /// <param name="microUpdate">更新对象</param>
        /// <returns></returns>
        public static IMicroUpdate RemoveUpdate(IMicroUpdate microUpdate)
        {
            if (microUpdate == null)
                return microUpdate;
            if (!InternalMicroData.cacheUpdateDict.TryGetValue(microUpdate, out IUpdateDescribe describe))
                return microUpdate;

            // 处理头节点
            if (InternalMicroData.updateDescribe == describe)
            {
                InternalMicroData.updateDescribe = describe.child;
                if (describe.child != null)
                    describe.child.parent = null;
            }
            // 处理尾节点
            if (InternalMicroData.lastUpdateDescribe == describe)
            {
                InternalMicroData.lastUpdateDescribe = describe.parent;
                if (describe.parent != null)
                    describe.parent.child = null;
            }
            // 处理中间节点
            else if (describe.parent != null)
            {
                describe.parent.child = describe.child;
                if (describe.child != null)
                    describe.child.parent = describe.parent;
            }

            // 清理节点
            describe.Release();

            // 从缓存中移除
            InternalMicroData.cacheUpdateDict.Remove(microUpdate);

            // 回收对象到对象池
            InternalMicroData.updatePool.Recover((MicroUpdateDescribe)describe);

            return microUpdate;
        }

        /// <summary>
        /// 添加一个逻辑更新
        /// </summary>
        /// <param name="microUpdate">逻辑更新对象</param>
        /// <param name="onCheckCanUpdate">检测当前对象是否可以更新,如果不提供默认可以更新</param>
        /// <returns></returns>
        public static IMicroLogicUpdate AddLogicUpdate(IMicroLogicUpdate microUpdate, Func<bool> onCheckCanUpdate = null)
        {
            if (microUpdate == null)
                return microUpdate;
            // 检查是否已经存在于更新列表中
            if (InternalMicroData.cacheLogicUpdateDict.TryGetValue(microUpdate, out IUpdateDescribe existDescribe))
            {
                // 如果已存在，更新检查事件并返回
                existDescribe.onCheckCanUpdateEvent += onCheckCanUpdate;
                return microUpdate;
            }

            MicroLogicUpdateDescribe describe = InternalMicroData.logicUpdatePool.Get();
            describe.microUpdate = microUpdate;
            describe.onCheckCanUpdateEvent += onCheckCanUpdate;
            // 添加到缓存字典
            InternalMicroData.cacheLogicUpdateDict.Add(microUpdate, describe);
            if (InternalMicroData.logicUpdateDescribe == null)
            {
                InternalMicroData.logicUpdateDescribe = describe;
                InternalMicroData.lastlogicUpdateDescribe = describe;
            }
            else
            {
                InternalMicroData.lastlogicUpdateDescribe.child = describe;
                describe.parent = InternalMicroData.lastlogicUpdateDescribe;
                InternalMicroData.lastlogicUpdateDescribe = describe;
            }
            return microUpdate;
        }
        /// <summary>
        /// 移除一个逻辑更新
        /// </summary>
        /// <param name="microUpdate">逻辑更新对象</param>
        /// <returns></returns>
        public static IMicroLogicUpdate RemoveLogicUpdate(IMicroLogicUpdate microUpdate)
        {
            if (microUpdate == null)
                return microUpdate;
            if (!InternalMicroData.cacheLogicUpdateDict.TryGetValue(microUpdate, out IUpdateDescribe describe))
                return microUpdate;

            // 处理头节点
            if (InternalMicroData.logicUpdateDescribe == describe)
            {
                InternalMicroData.logicUpdateDescribe = describe.child;
                if (describe.child != null)
                    describe.child.parent = null;
            }
            // 处理尾节点
            if (InternalMicroData.lastlogicUpdateDescribe == describe)
            {
                InternalMicroData.lastlogicUpdateDescribe = describe.parent;
                if (describe.parent != null)
                    describe.parent.child = null;
            }
            // 处理中间节点
            else if (describe.parent != null)
            {
                describe.parent.child = describe.child;
                if (describe.child != null)
                    describe.child.parent = describe.parent;
            }

            // 清理节点
            describe.Release();

            // 从缓存中移除
            InternalMicroData.cacheLogicUpdateDict.Remove(microUpdate);

            // 回收对象到对象池
            InternalMicroData.logicUpdatePool.Recover((MicroLogicUpdateDescribe)describe);


            return microUpdate;
        }

        /// <summary>
        /// 断言
        /// </summary>
        /// <param name="condition">条件</param>
        /// <exception cref="Exception"></exception>
        public static void Assert(bool condition)
        {
            Assert(condition, "");
        }
        /// <summary>
        /// 断言
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="message">断言失败消息</param>
        /// <exception cref="Exception"></exception>
        public static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception("Assert failure " + message ?? "");
            }
        }

        /// <summary>
        /// 启动一个协程
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            return _gameMono.StartCoroutine(routine);
        }

        /// <summary>
        /// 关闭一个协程
        /// </summary>
        /// <param name="routine"></param>
        public static void StopCoroutine(Coroutine routine)
        {
            _gameMono.StopCoroutine(routine);
        }

        /// <summary>
        /// 把方法抛到Unity线程执行
        /// </summary>
        /// <param name="action"></param>
        public static void RunOnUnityThread(Action action, object data = null)
        {
            //Promise.Run(() => action(), SynchronizationOption.Foreground).Forget();
            if (MainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                action();
            }
            else
            {
                mainSynchronizationContext.Post(_ => action(), data);
            }
        }

        /// <summary>
        /// 把方法抛到Unity线程执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="data"></param>
        public static void RunOnUnityThread<T>(Action<T> action, T data)
        {
            if (MainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                action(data);
            }
            else
            {
                mainSynchronizationContext.Post(_ => action(data), null);
            }
        }
        #endregion

        #region internal

        /// <summary>
        /// 获取一个模块的Mono
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <returns></returns>
        internal static ModuleDescribe GetModuleDescribe(string moduleName)
        {
            if (InternalMicroData.moduleAlias.ContainsKey(moduleName))
                return InternalMicroData.moduleAlias[moduleName].module;
            return null;
        }

        #endregion

        #region private

        /// <summary>
        /// 创建模块描述
        /// </summary>
        /// <param name="module"></param>
        private static void m_createModuleDescribe(IMicroModule module)
        {
            Type tType = module.GetType();
            ModuleDescribe describe = new ModuleDescribe(module);
            string[] aliasNames = m_getModuleAliasNames(tType);
            foreach (var item in aliasNames)
            {
                ModuleAliasDescribe linkedNode = new ModuleAliasDescribe(describe);
                if (InternalMicroData.moduleAlias.ContainsKey(item))
                {
                    ModuleAliasDescribe temp = InternalMicroData.moduleAlias[item];
                    linkedNode.child = temp;
                }
                InternalMicroData.moduleAlias.Add(item, linkedNode);
            }
            InternalMicroData.allModuleContainer.Add(describe);
        }

        /// <summary>
        /// 获取模块的所有别名,含自身名字
        /// </summary>
        /// <param name="moduleType">模块类型</param>
        /// <returns></returns>
        private static string[] m_getModuleAliasNames(Type moduleType)
        {
            List<string> names = getAliasNameList();
            names.Add(moduleType.FullName);
            Type[] interfaceTypes = moduleType.GetInterfaces();
            foreach (var interfaceType in interfaceTypes)
            {
                if (interfaceType != MODULE_INTERFACE_TYPE && MODULE_INTERFACE_TYPE.IsAssignableFrom(interfaceType))
                {
                    names.Add(interfaceType.FullName);
                }
            }
            // 只检查父类类型名称
            Type baseType = moduleType.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                if (MODULE_INTERFACE_TYPE.IsAssignableFrom(baseType))
                {
                    names.Add(baseType.FullName);
                }
                baseType = baseType.BaseType;
            }
            return names.ToArray();
        }

        /// <summary>
        /// 暂停依赖moduleDescribe的Module
        /// </summary>
        /// <param name="moduleDescribe"></param>
        private static void m_suspendModuleDependency(ModuleDescribe moduleDescribe)
        {
            for (int i = moduleDescribe.UsedDescribes.Count - 1; i >= 0; --i)
            {
                ModuleDescribe item = moduleDescribe.UsedDescribes[i];
                if (item.State != ModuleState.Running)
                    continue;
                //item.DelDependency(moduleDescribe);
                try
                {
                    item.OnSuspend();
                    m_suspendModuleDependency(item);
                }
                catch (Exception ex)
                {
                    logger.LogError($"{moduleDescribe.Target.GetType().Name}:OnSuspend执行失败,错误信息:{ex.Message}");
                }
            }
        }

        /// <summary>
        /// 初始化模块
        /// </summary>
        /// <param name="moduleDescribe">模块的</param>
        /// <exception cref="NotImplementedException"></exception>
        private static void m_initModule(ModuleDescribe moduleDescribe)
        {
            if (moduleDescribe.IsReady)
            {
                try
                {
                    moduleDescribe.Init();
                }
                catch (Exception ex)
                {
                    logger.LogError($"{moduleDescribe.Target.GetType().Name}:初始化执行失败,错误信息:{ex.Message}");
                }
            }
        }

        /// <summary>
        /// 重启模块
        /// </summary>
        /// <param name="moduleDescribe"></param>
        private static void m_resumeModule(ModuleDescribe moduleDescribe)
        {
            if (!moduleDescribe.IsReady)
                return;
            try
            {
                moduleDescribe.OnResume();
            }
            catch (Exception ex)
            {
                logger.LogError($"{moduleDescribe.Target.GetType().Name}:OnResume执行失败,错误信息:{ex.Message}");
            }
        }

        /// <summary>
        /// 释放所有模块
        /// </summary>
        private static void m_releaseModules()
        {
            foreach (var item in InternalMicroData.allModuleContainer)
            {
                item.Destroy();
            }
            InternalMicroData.allModuleContainer.Clear();
            InternalMicroData.moduleAlias.Clear();
        }

        /// <summary>
        /// 获取模块的链表节点
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        private static ModuleAliasDescribe m_getModuleDescribeLinkedNode(string moduleName)
        {
            if (InternalMicroData.moduleAlias.ContainsKey(moduleName))
                return InternalMicroData.moduleAlias[moduleName];
            return null;
        }
        /// <summary>
        /// 仅仅移除一个模块的描述
        /// 不做任何操作
        /// </summary>
        /// <param name="moduleDescribe"></param>
        private static void m_onlyRemoveModule(ModuleDescribe moduleDescribe)
        {
            InternalMicroData.allModuleContainer.Remove(moduleDescribe);
        }

        /// <summary>
        /// 获取别名列表,避免重复创建
        /// </summary>
        /// <returns></returns>
        private static List<string> getAliasNameList()
        {
            if (aliasNameList == null)
                aliasNameList = new List<string>();
            else
                aliasNameList.Clear();
            return aliasNameList;
        }


        private static void m_update()
        {
            try
            {
                onUpdate?.Invoke(_updateDeltaTime);
                int count = InternalMicroData.allModuleContainer.Count;
                bool allReady = true;
                for (int i = 0; i < count; i++)
                {
                    ModuleDescribe moduleDescribe = InternalMicroData.allModuleContainer[i];
                    try
                    {
                        switch (moduleDescribe.State)
                        {
                            case ModuleState.None:
                                m_initModule(moduleDescribe);
                                allReady = false;
                                break;
                            case ModuleState.Initializing:
                                moduleDescribe.CheckInit();
                                allReady = false;
                                break;
                            case ModuleState.Suspended:
                                m_resumeModule(moduleDescribe);
                                break;
                            case ModuleState.Destory:
                                m_suspendModuleDependency(moduleDescribe);
                                InternalMicroData.allModuleContainer.Remove(moduleDescribe);
                                break;
                            default:
                                break;
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"{moduleDescribe.Target.GetType().Name}:{moduleDescribe.State}执行失败,错误信息:{ex.Message}");
                    }
                }
                IUpdateDescribe describe = InternalMicroData.updateDescribe;
                while (describe != null)
                {
                    try
                    {
                        describe.OnExecute(_updateDeltaTime);
                        describe = describe.child;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"{describe.ToString()}:OnUpdate失败,错误信息:{ex.Message}");
                    }
                }
                if (!_isModuleReady && allReady)
                {
                    try
                    {
                        _isModuleReady = true;
                        _onModuleReady?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.Message);
                        throw new Exception("onModuleReady调用失败");
                    }
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }


        private static void m_logicUpdate()
        {
            try
            {
                float logicDeltaTime = MicroSetting.logicDeltaTime;
                int logicCount = 0;
                while (_logicUpdateRecordTime - logicDeltaTime >= 0)
                {
                    _logicUpdateRecordTime -= logicDeltaTime;
                    logicCount = logicCount + 1;
                    onLogicUpdate?.Invoke(logicDeltaTime);
                }
                _logicUpdateRecordTime += _updateDeltaTime;
                IUpdateDescribe describe = InternalMicroData.logicUpdateDescribe;
                while (describe != null)
                {
                    try
                    {
                        describe.OnExecute(logicDeltaTime);
                        describe = describe.child;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"{describe.ToString()}:OnLogicUpdate失败,错误信息:{ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }

        private static void m_addLogicUpdate()
        {
            //增加一个循环
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            int index = 0;
            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                if (playerLoop.subSystemList[i].type == typeof(EarlyUpdate))
                {
                    index = i;
                    break;
                }
            }
            PlayerLoopSystem[] preUpdateList = playerLoop.subSystemList[index].subSystemList.ToArray();
            PlayerLoopSystem[] dest = new PlayerLoopSystem[preUpdateList.Length + 1];
            dest[0] = new PlayerLoopSystem()
            {
                type = typeof(LogicUpdateLoop),
                updateDelegate = m_logicUpdate
            };
            for (int i = 0; i < preUpdateList.Length; i++)
            {
                dest[i + 1] = preUpdateList[i];
            }

            playerLoop.subSystemList[index].subSystemList = dest;
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static void m_removeLogicUpdate()
        {
            //删除一个循环
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            int index = 0;
            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                if (playerLoop.subSystemList[i].type == typeof(EarlyUpdate))
                {
                    index = i;
                    break;
                }
            }
            PlayerLoopSystem[] preUpdateList = playerLoop.subSystemList[index].subSystemList.ToArray();
            PlayerLoopSystem[] dest = new PlayerLoopSystem[preUpdateList.Length - 1];
            for (int i = 0; i < dest.Length; i++)
            {
                dest[i] = preUpdateList[i + 1];
            }
            playerLoop.subSystemList[index].subSystemList = dest;
            PlayerLoop.SetPlayerLoop(playerLoop);
        }
        #endregion

        #region CustomUpdate

        private struct LogicUpdateLoop
        {
        }
        #endregion

        #region class

        private class GameContextMono : MonoBehaviour
        {
            private void Awake()
            {
                m_addLogicUpdate();
            }
            private void Update()
            {
                _updateDeltaTime = Time.deltaTime;
                m_update();
            }
            private void OnDestroy()
            {
                m_releaseModules();
                m_removeLogicUpdate();
            }
        }

        #endregion
    }

    partial class MicroContext
    {
        /// <summary>
        /// 游戏时间
        /// </summary>
        public static float GameTime => Time.realtimeSinceStartup;
    }
}
