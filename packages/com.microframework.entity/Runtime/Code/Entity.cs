using MFramework.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MFramework.Runtime
{
#if UNITY_EDITOR
    public class EditorEntityMono : MonoBehaviour
    {
        public Entity Entity { get; internal set; }
    }
#endif

    /// <summary>
    /// 实体
    /// </summary>
    public sealed partial class Entity
    {
        /// <summary>
        /// 当前实体的参数
        /// </summary>
        private BindableDictionary<string, object> _args = new BindableDictionary<string, object>();

        private DelayDictionary<Type, EntityComponent> _coms = new DelayDictionary<Type, EntityComponent>();

        /// <summary>
        /// 当前实体的根Tran
        /// </summary>
        public Transform RootTran { get; private set; }
        /// <summary>
        /// 当前实体的根GameObject
        /// </summary>
        public GameObject RootObj { get; private set; }

        private EntityWorld _world;
        /// <summary>
        /// 当前实体所在的世界
        /// </summary>
        public EntityWorld World => _world;

        private Entity()
        {
            this.RootObj = new GameObject("Entity");
            this.RootTran = this.RootObj.transform;
        }
        /// <summary>
        /// 初始化实体
        /// <para>可以指定名称和父节点</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        public void Init(string name, Transform parent)
        {
#if UNITY_EDITOR
            RootObj.AddComponent<EditorEntityMono>().Entity = this;
#endif
            this.RootObj.name = name;
            this.RootTran.SetParent(parent);
        }
        public T AddComponent<T>() where T : EntityComponent, new()
        {
            Type comType = typeof(T);
            if (_coms.ContainsKey(comType))
            {
                return _coms[comType] as T;
            }
            T t = s_createComponent<T>();
            t.state = ComponentState.None;
            t.entity = this;
            _coms.Add(comType, t);
            s_initComponent(this, t);
            return t;
        }
        public bool RemoveComponent<T>() where T : EntityComponent, new()
        {
            Type comType = typeof(T);
            if (_coms.TryGetValue(comType, out EntityComponent component))
            {
                component.state = ComponentState.Destory;
                component.OnDestroy();
                _coms.Remove(comType);
                s_recoverComponent<T>(component);
                s_suspendComponent(this, component);
                return true;
            }
            return false;
        }

        public T GetArgs<T>(string argName, T defaultValue = default)
        {
            if (_args.ContainsKey(argName))
                return (T)_args[argName];
            else
                _args.Add(argName, defaultValue);
            return defaultValue;
        }
        public void SetArgs<T>(string argName, T value)
        {
            if (_args.ContainsKey(argName))
                _args[argName] = value;
            else
                _args.Add(argName, value);
        }
        public void Subscribe(string argName, BindableDelegate<object> action, bool firstNotify = false, bool notifyAnyway = false)
        {
            _args.Subscribe(argName, action, firstNotify, notifyAnyway);
        }
        public void Unsubscribe(string argName, BindableDelegate<object> action)
        {
            _args.Unsubscribe(argName, action);
        }
        public void Publish(string argName)
        {
            _args.Publish(argName);
        }
        /// <summary>
        /// 逻辑Update，一般慢于Update
        /// <para>用来Init组件和处理逻辑</para>
        /// </summary>
        /// <param name="deltaTime"></param>
        public void OnLogicUpdate(float deltaTime)
        {
            foreach (var item in _coms)
            {
                try
                {
                    if (item.Value.state == ComponentState.Running)
                        item.Value.OnLogicUpdate(deltaTime);

                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }
        }

        /// <summary>
        /// Unity的Update
        /// </summary>
        /// <param name="deltaTime"></param>
        public void OnUpdate(float deltaTime)
        {
            foreach (var item in _coms)
            {
                try
                {
                    switch (item.Value.state)
                    {
                        case ComponentState.None:
                            s_initComponent(this, item.Value);
                            break;
                        case ComponentState.Initializing:
                            if (item.Value.IsInit) item.Value.state = ComponentState.Running;
                            break;
                        case ComponentState.Suspended:
                            s_resumeComponent(this, item.Value);
                            break;
                        case ComponentState.Running:
                            if (item.Value.IsInit)
                                item.Value.OnUpdate(deltaTime);
                            else
                            {
                                item.Value.state = ComponentState.Initializing;
                                s_suspendComponent(this, item.Value);
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }
        }

        public override int GetHashCode()
        {
            return this.RootObj.GetInstanceID();
        }

        /// <summary>
        /// 释放
        /// </summary>
        internal void Release()
        {
#if UNITY_EDITOR
            GameObject.Destroy(RootObj.GetComponent<EditorEntityMono>());
#endif
            _args.UnsubscribeAll();
            foreach (var item in _coms)
            {
                item.Value.state = ComponentState.Destory;
                item.Value.OnDestroy();
            }
            _coms.Clear();
            _args.Clear();
        }
    }
    /// <summary>
    /// 实体类静态部分
    /// </summary>
    partial class Entity
    {
        /// <summary>
        /// 创建一个实体
        /// </summary>
        /// <returns></returns>
        public static Entity Create() => Create(null, null, EntityWorld.GetWorld());
        /// <summary>
        /// 创建一个实体
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public static Entity Create(EntityWorld world) => Create(null, null, world);
        /// <summary>
        /// 创建一个实体
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Entity Create(string name) => Create(name, null, EntityWorld.GetWorld());
        /// <summary>
        /// 创建一个实体
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Entity Create(Transform parent) => Create(null, parent, EntityWorld.GetWorld());
        /// <summary>
        /// 创建一个实体
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Entity Create(string name, Transform parent) => Create(name, parent, EntityWorld.GetWorld());
        /// <summary>
        /// 创建一个实体
        /// </summary>
        /// <param name="name"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public static Entity Create(string name, EntityWorld world) => Create(name, null, world);
        /// <summary>
        /// 创建一个实体
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public static Entity Create(Transform parent, EntityWorld world) => Create(null, parent, world);
        /// <summary>
        /// 创建一个实体
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public static Entity Create(string name, Transform parent, EntityWorld world)
        {
            string entityName = string.IsNullOrWhiteSpace(name) ? "Entity" : name;
            Entity entity = _entityPool.Get();
            entity.RootObj.SetActive(true);
            entity._world = world;
            world.AddEntity(entity);
            entity.Init(entityName, parent ?? world.WorldTran);
            return entity;
        }
        public static void Recover(Entity entity)
        {
            EntityWorld world = entity._world;
            entity._world = null;
            if (world != null)
            {
                world.RemoveEntity(entity);
            }
            _entityPool.Recover(entity);
        }

        /// <summary>
        /// 实体的对象池
        /// </summary>
        private static UnityPool<Entity> _entityPool = new UnityPool<Entity>();
        /// <summary>
        /// 组件对象池字典
        /// </summary>
        private static Dictionary<Type, MicroPool<EntityComponent>> _comPoolDic = new Dictionary<Type, MicroPool<EntityComponent>>();
        /// <summary>
        /// 组件类型依赖的类型
        /// </summary>
        private static Dictionary<Type, List<Type>> s_comDepDic = new Dictionary<Type, List<Type>>();
        static Entity()
        {
            _entityPool.Name = "EntityPool";
            _entityPool.onCreate += () => new Entity();
            _entityPool.onRecover += (e) =>
            {
                e.RootObj.SetActive(false);
                e.RootTran.SetParent(_entityPool.transform);
                e.Release();
            };
        }

        /// <summary>
        /// 创建组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static T s_createComponent<T>() where T : EntityComponent, new()
        {
            Type type = typeof(T);
            MicroPool<EntityComponent> pool = null;
            if (!_comPoolDic.TryGetValue(type, out pool))
            {
                pool = new MicroPool<EntityComponent>(() => new T());
                _comPoolDic[type] = pool;
            }
            return pool.Get() as T;
        }
        /// <summary>
        /// 回收组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        private static void s_recoverComponent<T>(EntityComponent component) where T : EntityComponent, new()
        {
            Type type = typeof(T);
            MicroPool<EntityComponent> pool = null;
            if (!_comPoolDic.TryGetValue(type, out pool))
            {
                pool = new MicroPool<EntityComponent>(() => new T());
                _comPoolDic[type] = pool;
            }
            pool.Recover(component);
        }

        /// <summary>
        /// 初始化组件的依赖类型
        /// </summary>
        /// <param name="comType"></param>
        /// <param name="depTypes"></param>
        private static void s_initComDepTypes(Type comType, ref List<Type> depTypes)
        {
            Type baseType = typeof(EntityComponent);
            depTypes = new List<Type>();
            RequireTypeAttribute requireAttr = comType.GetCustomAttribute<RequireTypeAttribute>();
            if (requireAttr != null && requireAttr.RequireTypes != null)
            {
                foreach (Type type in requireAttr.RequireTypes)
                {
                    if (!type.IsAbstract && type.IsSubclassOf(baseType))
                    {
                        depTypes.Add(type);
                    }
                }
            }
        }

        /// <summary>
        /// 组件是否可以初始化
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        private static bool s_comIsReady(Entity entity, EntityComponent com)
        {
            Type comType = com.GetType();
            List<Type> depTypes = null;
            if (!s_comDepDic.TryGetValue(comType, out depTypes))
            {
                s_initComDepTypes(comType, ref depTypes);
                s_comDepDic.Add(comType, depTypes);
            }
            foreach (var item in depTypes)
            {
                if (entity._coms.TryGetValue(item, out EntityComponent value))
                {
                    if (value.state != ComponentState.Running)
                    {
                        return false;
                    }
                }
                else
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="com"></param>
        private static void s_initComponent(Entity entity, EntityComponent com)
        {
            if (s_comIsReady(entity, com))
            {
                try
                {
                    com.OnInit();
                    com.state = ComponentState.Initializing;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        /// <summary>
        /// 检查一个组件是否可以恢复使用
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="com"></param>
        private void s_resumeComponent(Entity entity, EntityComponent com)
        {
            if (s_comIsReady(entity, com))
            {
                try
                {
                    com.OnResume();
                    com.state = ComponentState.Running;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        /// <summary>
        /// 停止实体上面依赖com的组件
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="com">组件</param>
        private static void s_suspendComponent(Entity entity, EntityComponent com)
        {
            Type comType = com.GetType();
            foreach (var item in entity._coms)
            {
                var itemType = item.Value.GetType();
                if (comType == itemType)
                {
                    continue;
                }
                if (s_comDepDic.TryGetValue(itemType, out List<Type> depTypes))
                {
                    if (depTypes.Contains(comType))
                    {
                        item.Value.state = ComponentState.Suspended;
                        item.Value.OnSuspend();
                    }
                }
            }
        }
    }
}
