using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.Runtime
{
    /// <summary>
    /// 资源路径处理器
    /// </summary>
    public interface IResourcePathProcessor
    {
        /// <summary>
        /// 路径是否有效
        /// </summary>
        /// <param name="originPath">原始路径，既AssetDatabase读取的路径，含后缀</param>
        /// <returns></returns>
        bool IsValid(string originPath);

        /// <summary>
        /// 获取资源路径
        /// </summary>
        /// <param name="originPath">原始路径，既AssetDatabase读取的路径，含后缀</param>
        /// <returns></returns>
        string GetResourcePath(string originPath);
    }
}
