using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    /// <summary>
    /// 自定义绘制类型
    /// </summary>
    public enum CustomDrawerType
    {
        /// <summary>
        /// 在Basics前装饰
        /// </summary>
        PreDecorate,
        /// <summary>
        /// 基础的类型,替换原本的类型渲染
        /// </summary>
        Basics,
        /// <summary>
        /// 修改原本的类型渲染，会在最后绘制
        /// </summary>
        Modify,
        /// <summary>
        /// 在Basics后装饰
        /// </summary>
        NextDecorate,
    }

    /// <summary>
    /// 自定义绘制接口
    /// 如果存在多个Basics,则只绘制其中一个,其他的忽略
    /// </summary>
    public interface ICustomDrawer
    {
        /// <summary>
        /// 绘制类型
        /// </summary>
        CustomDrawerType DrawerType { get; }
        /// <summary>
        /// 执行优先级默认时0,优先级越大越先绘制
        /// </summary>
        int Priority => 0;
        /// <summary>
        /// 调用此方法进行绘制
        /// </summary>
        /// <param name="target"></param>
        /// <param name="fieldInfo"></param>
        /// <param name="getValue"></param>
        /// <param name="setValue"></param>
        /// <param name="originalElement"></param>
        /// <returns></returns>
        VisualElement DrawUI(object target, FieldInfo fieldInfo, Func<object> getValue, Action<object> setValue, VisualElement originalElement = null) => null;
        //VisualElement DrawUI(MicroObjectField objectField, Func<object> getValue, Action<object> setValue) => null;
    }
}