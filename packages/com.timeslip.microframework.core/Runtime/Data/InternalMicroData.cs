using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        internal static DelayDictionary<string, ModuleDescribeLinkedNode> moduleAlias = new DelayDictionary<string, ModuleDescribeLinkedNode>();
        /// <summary>
        /// 所有模块
        /// </summary>
        internal readonly static DelayList<ModuleDescribe> allModuleContainer = new DelayList<ModuleDescribe>();

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
    }
}
