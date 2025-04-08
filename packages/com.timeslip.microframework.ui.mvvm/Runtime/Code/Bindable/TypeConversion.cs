using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace MFramework.Runtime
{
    /// <summary>
    /// 类型转换
    /// </summary>
    internal sealed class TypeConversion
    {
        private Dictionary<Type, object> conversions = new Dictionary<Type, object>();
        private readonly Type _classType = null;
        public TypeConversion(Type classType)
        {
            _classType = classType;
        }
        /// <summary>
        /// 添加一个转换
        /// </summary>
        /// <param name="parameterType">方法参数类型</param>
        /// <param name="method">方法</param>
        public void AddConversion(Type parameterType, MethodInfo method)
        {
            if (conversions.ContainsKey(parameterType))
            {
                throw new ArgumentException(" An item with the same key has already been added.key:" + parameterType);
            }
            ParameterExpression param = Expression.Parameter(parameterType, "a");
            Type @delegate = typeof(Func<,>).MakeGenericType(parameterType, _classType);
            object o = Expression.Lambda(@delegate, Expression.Call(method, param), param).Compile();
            conversions.Add(parameterType, o);
        }
        /// <summary>
        /// 添加一个转换
        /// </summary>
        /// <param name="parameterType">方法参数类型</param>
        /// <param name="method">方法</param>
        public object GetConversion(Type parameterType)
        {
            if (conversions.ContainsKey(parameterType))
            {
                return conversions[parameterType];
            }
            return null;
        }
    }
}
