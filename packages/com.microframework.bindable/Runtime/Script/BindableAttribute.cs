using System;

namespace MFramework.Runtime
{
    /// <summary>
    /// 需要变成绑定属性的字段
    /// 该字段需要是私有的
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class BindableFieldAttribute : Attribute
    {
        /// <summary>
        /// 注释
        /// </summary>
        public string Summary = "";

        /// <summary>
        /// 使用驼峰命名法生成绑定属性
        /// </summary>
        public BindableFieldAttribute() : this(default) { }
        /// <summary>
        /// 使用指定名称生成绑定属性
        /// <para>如果名称为空，则用驼峰生成</para>
        /// </summary>
        public BindableFieldAttribute(string summary) => Summary = summary;
    }
}
