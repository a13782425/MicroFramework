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
        /// 在Basics后装饰
        /// </summary>
        Post‌Decorate,
    }

    /// <summary>
    /// 自定义绘制接口
    /// 如果存在多个Basics,则只绘制其中一个,其他的忽略
    /// </summary>
    public interface ICustomDrawer : IConstructor, IComparer<ICustomDrawer>, IComparable<ICustomDrawer>
    {
        /// <summary>
        /// 绘制类型
        /// </summary>
        CustomDrawerType DrawerType { get; }
        /// <summary>
        /// 调用此方法进行绘制
        /// </summary>
        /// <param name="objectField"></param>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        VisualElement DrawUI(MicroObjectField objectField, FieldInfo fieldInfo);
        int IComparer<ICustomDrawer>.Compare(ICustomDrawer x, ICustomDrawer y) => x.DrawerType.CompareTo(y.DrawerType);
        int IComparable<ICustomDrawer>.CompareTo(ICustomDrawer other) => this.DrawerType.CompareTo(other.DrawerType);
    }
}