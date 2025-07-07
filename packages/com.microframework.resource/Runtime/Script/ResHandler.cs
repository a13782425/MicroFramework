using MFramework.Task;
using UnityEngine;

namespace MFramework.Runtime
{
    /// <summary>
    /// 异步加载的资源句柄
    /// </summary>
    public class ResHandler : IMicroTaskInstruction<Object>
    {
        public ResHandler()
        {
            IsDone = false;
            IsCancel = false;
            Asset = null;
            ErrorMessage = null;
        }
        /// <summary>
        /// 加载是否完成
        /// </summary>
        public bool IsDone { get; set; }
        /// <summary>
        /// 是否取消加载
        /// </summary>
        public bool IsCancel { get; set; }
        /// <summary>
        /// 加载完成的资源
        /// </summary>
        public Object Asset { get; set; }

        /// <summary>
        /// 加载错误信息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 是否加载错误
        /// </summary>
        public bool IsError => !string.IsNullOrWhiteSpace(ErrorMessage);

        public Object GetResult()
        {
            return Asset;
        }

        public bool IsCompleted()
        {
            return IsDone || IsCancel;
        }
    }
}
