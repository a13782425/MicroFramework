using MFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MFramework.Runtime
{
    /// <summary>
    /// 实体世界
    /// </summary>
    public sealed class EntityWorld
    {
        private string _worldName;
        /// <summary>
        /// 当前世界的名字
        /// </summary>
        public string WorldName => _worldName;
        /// <summary>
        /// 所有实体的集合
        /// </summary>
        private Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        private GameObject _worldObj;
        /// <summary>
        /// 当前实体世界的树节点
        /// </summary>
        public GameObject WorldObj => _worldObj;
        /// <summary>
        /// 当前实体世界的树节点
        /// </summary>
        public Transform WorldTran => _worldObj.transform;


        private EntityWorld(string worldName)
        {
            _worldName = worldName;
        }

        /// <summary>
        /// 创建一个实体
        /// </summary>
        /// <returns></returns>
        public Entity Create() => Create(null, null);
        /// <summary>
        /// 创建一个实体
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Entity Create(string name) => Create(name, null);
        /// <summary>
        /// 创建一个实体
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public Entity Create(Transform parent) => Create(null, parent);
        /// <summary>
        /// 创建一个实体
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public Entity Create(string name, Transform parent)
        {
            return Entity.Create(name, parent, this);
        }

        public void Recover(Entity entity)
        {
            Entity.Recover(entity);
        }
        internal void AddEntity(Entity entity)
        {
            if (!entities.ContainsKey(entity.GetHashCode()))
            {
                entities.Add(entity.GetHashCode(), entity);
            }
        }
        internal void RemoveEntity(Entity entity)
        {
            if (entities.ContainsKey(entity.GetHashCode()))
            {
                entities.Remove(entity.GetHashCode());
            }
        }
        private void OnInit()
        {
            _worldObj = new GameObject(WorldName);
            MicroContext.onLogicUpdate += MicroContext_onLogicUpdate;
            MicroContext.onUpdate += MicroContext_onUpdate;
        }

        private void OnDestroy()
        {
            MicroContext.onLogicUpdate -= MicroContext_onLogicUpdate;
            MicroContext.onUpdate -= MicroContext_onUpdate;
            if (this.WorldObj != null)
            {
                foreach (var item in entities.Values.ToList())
                {
                    Recover(item);
                }
            }
            entities.Clear();
        }

        private void MicroContext_onUpdate(float deltaTime)
        {
            foreach (var item in entities)
            {
                try
                {
                    item.Value.OnUpdate(deltaTime);
                }
                catch (Exception ex)
                {
                    logger.LogError($"实体{item.Value.RootTran.name},OnUpdate执行失败:{ex.Message}");
                }
            }
        }

        private void MicroContext_onLogicUpdate(float deltaTime)
        {
            foreach (var item in entities)
            {
                try
                {
                    item.Value.OnLogicUpdate(deltaTime);
                }
                catch (Exception ex)
                {
                    logger.LogError($"实体{item.Value.RootTran.name},OnLogicUpdate执行失败:{ex.Message}");
                }
            }
        }


        #region static

        static EntityWorld()
        {
            logger = MicroLogger.GetMicroLogger(nameof(EntityWorld));
            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
            GetDontDestroyWorld();
        }

        private static void SceneManager_sceneUnloaded(Scene scene)
        {
            if (!scene.IsValid())
                return;
            DestroyWorld(scene.name);
        }

        private static IMicroLogger logger = null;
        /// <summary>
        /// 全部世界
        /// </summary>
        private static Dictionary<string, EntityWorld> _allWorld = new Dictionary<string, EntityWorld>() { };
        /// <summary>
        /// 获取当前处于激活场景的世界
        /// </summary>
        /// <returns></returns>
        public static EntityWorld GetWorld()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                logger.LogError("当前场景不是有效的");
                return null;
            }
            return GetWorld(scene.name);
        }
        /// <summary>
        /// 获取一个自定义名字的世界
        /// </summary>
        /// <param name="worldName"></param>
        /// <returns></returns>
        public static EntityWorld GetWorld(string worldName)
        {
            if (string.IsNullOrWhiteSpace(worldName))
            {
                logger.LogError("实体世界名字不能为空");
                return null;
            }
            if (_allWorld.TryGetValue(worldName, out EntityWorld world))
            {
                return world;
            }
            world = new EntityWorld(worldName);
            _allWorld[worldName] = world;
            world.OnInit();
            return world;
        }

        /// <summary>
        /// 销毁一个世界
        /// </summary>
        public static void DestroyWorld(string worldName)
        {
            if (string.IsNullOrWhiteSpace(worldName))
            {
                logger.LogError("实体世界名字不能为空");
                return;
            }
            if (!_allWorld.TryGetValue(worldName, out EntityWorld world))
            {
                return;
            }
            DestroyWorld(world);
        }
        /// <summary>
        /// 销毁一个世界
        /// </summary>
        public static void DestroyWorld(EntityWorld world)
        {
            _allWorld.Remove(world.WorldName);
            world.OnDestroy();
        }
        /// <summary>
        /// 获取永不销毁的世界
        /// </summary>
        /// <returns></returns>
        public static EntityWorld GetDontDestroyWorld()
        {
            string worldName = "DontDestroyWorld";
            if (_allWorld.TryGetValue(worldName, out EntityWorld world))
            {
                return world;
            }
            world = new EntityWorld(worldName);
            _allWorld[worldName] = world;
            world.OnInit();
            GameObject.DontDestroyOnLoad(world.WorldObj);
            return world;
        }
        #endregion
    }
}
