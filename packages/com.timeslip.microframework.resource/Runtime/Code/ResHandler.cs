using MFramework.Core;
using UnityEngine;

namespace MFramework.Runtime
{
    /// <summary>
    /// 异步加载的资源句柄
    /// </summary>
    public class ResHandler : IAwaitInstruction
    {
        internal protected ResHandler()
        {
            isDone = false;
            isCancel = false;
            asset = null;
        }
        /// <summary>
        /// 加载是否完成
        /// </summary>
        public bool isDone { get; internal protected set; }
        /// <summary>
        /// 是否取消加载
        /// </summary>
        public bool isCancel { get; set; }
        /// <summary>
        /// 加载完成的资源
        /// </summary>
        public Object asset { get; internal protected set; }

        public bool IsCompleted() => isDone || isCancel;
    }
}
