using System;

namespace MFramework.Runtime
{
    /// <summary>
    /// mvvm的类型转换
    /// <para>作用于静态方法static typeA abc(typeB a)</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TypeConversionAttribute : Attribute
    {
    }
}
