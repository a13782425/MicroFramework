using System;
using System.Collections.Generic;

namespace MFramework.Core
{
    /// <summary>
    /// 微框架的静态数据
    /// </summary>
    internal static class InternalMicroData
    {
        /// <summary>
        /// 模块的别名
        /// </summary>
        internal static Dictionary<string, ModuleAliasDescribe> moduleAlias = new Dictionary<string, ModuleAliasDescribe>();
        /// <summary>
        /// 所有模块
        /// </summa>
        internal readonly static List<ModuleDescribe> allModuleContainer = new List<ModuleDescribe>();

        /// <summary>
        /// 当前模块
        /// </summary>
        internal static ModuleDescribe moduleDescribe = default;

        /// <summary>
        /// 所有模块的缓存
        /// </summary>
        internal readonly static Dictionary<Type, ModuleDescribe> cacheModuleDict = new Dictionary<Type, ModuleDescribe>();

        #region 更新
        /// <summary>
        /// 更新对象开头
        /// </summary>
        internal static IUpdateDescribe updateDescribe = default;

        /// <summary>
        /// 更新对象结尾
        /// </summary>
        internal static IUpdateDescribe lastUpdateDescribe = default;

        /// <summary>
        /// 所有更新的模块的缓存
        /// </summary>
        internal static readonly Dictionary<IMicroUpdate, IUpdateDescribe> cacheUpdateDict = new Dictionary<IMicroUpdate, IUpdateDescribe>();

        /// <summary>
        /// update描述对象池
        /// </summary>
        internal static MicroPool<MicroUpdateDescribe> updatePool = new MicroPool<MicroUpdateDescribe>(() => new MicroUpdateDescribe());
        #endregion

        #region 逻辑更新
        /// <summary>
        /// 逻辑更新对象开头
        /// </summary>
        internal static IUpdateDescribe logicUpdateDescribe = default;
        /// <summary>
        /// 逻辑更新对象结尾
        /// </summary>
        internal static IUpdateDescribe lastlogicUpdateDescribe = default;
        /// <summary>
        /// 所有更新的模块
        /// </summary>
        internal static readonly Dictionary<IMicroLogicUpdate, IUpdateDescribe> cacheLogicUpdateDict = new Dictionary<IMicroLogicUpdate, IUpdateDescribe>();

        /// <summary>
        /// logicUpdate描述对象池
        /// </summary>
        internal static MicroPool<MicroLogicUpdateDescribe> logicUpdatePool = new MicroPool<MicroLogicUpdateDescribe>(() => new MicroLogicUpdateDescribe());
        #endregion
    }
}
