using System;

namespace MFramework.Core
{
    /// <summary>
    /// 需要变成绑定属性的字段
    /// 该字段需要是私有的
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class BindableFieldAttribute : Attribute
    {
        public string Name = default;

        /// <summary>
        /// 注释
        /// </summary>
        public string Summary = "";
        ///// <summary>
        ///// 通知前代码
        ///// </summary>
        //public string PrePublish= "";
        ///// <summary>
        ///// 通知后代码
        ///// </summary>
        //public string AftPublish = "";
        /// <summary>
        /// 使用驼峰命名法生成绑定属性
        /// </summary>
        public BindableFieldAttribute() : this(default) { }
        /// <summary>
        /// 使用指定名称生成绑定属性
        /// <para>如果名称为空，则用驼峰生成</para>
        /// </summary>
        public BindableFieldAttribute(string name) => Name = name;
    }

    /// <summary>
    /// 属于映射
    /// 用于主动生成SetData方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public sealed class BindableMappingAttribute : Attribute
    {
        public string Content = "";

        /// <summary>
        /// 注释
        /// </summary>
        public Type MappingType;
        /// <summary>
        /// 是否使用语句
        /// <para>如果使用语句 content将原封不动的使用</para>    
        /// <para>参数名：data</para>
        /// </summary>
        public bool UseStatement = false;
        /// <summary>
        /// 映射目标对象的属性或者字段名
        /// </summary>
        public BindableMappingAttribute(Type mappingType) : this(mappingType, "") { }
        public BindableMappingAttribute(Type mappingType, string content)
        {
            this.Content = content;
            this.MappingType = mappingType;
        }
    }
}
