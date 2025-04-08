using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MFramework.Core.MicroContext;

namespace MFramework.Core
{
    /// <summary>
    /// 模块拓展
    /// </summary>
    public static class ModuleExtensions
    {
        /// <summary>
        /// 获取一个模块的状态
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public static ModuleState GetState(this IMicroModule module)
        {
            ModuleDescribe describe = GetModuleDescribe(module.GetType().FullName);
            Assert(describe != null);
            return describe.State;
        }

        /// <summary>
        /// 获取一个模块的Trans
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public static Transform GetTransform(this IMicroModule module)
        {
            ModuleDescribe describe = GetModuleDescribe(module.GetType().FullName);
            Assert(describe != null);
            return describe.gameObject.transform;
        }
        /// <summary>
        /// 获取一个模块的日志
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public static IMicroLogger GetMicroLogger(this IMicroModule module)
        {
            return MicroLogger.GetMicroLogger(module.GetType().Name);
        }
    }
}
