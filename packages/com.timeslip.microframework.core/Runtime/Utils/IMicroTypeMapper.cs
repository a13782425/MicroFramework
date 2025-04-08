using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.Core
{
    /// <summary>
    /// 微框架类型映射器接口
    /// </summary>
    public interface IMicroTypeMapper
    {
        Type GetType(string typeFullName);

        Type GetType(MicroClassSerializer classSerializer);

    }
}
