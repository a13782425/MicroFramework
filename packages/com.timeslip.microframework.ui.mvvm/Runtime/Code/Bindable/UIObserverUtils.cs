using MFramework.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MFramework.Runtime
{

    /// <summary>
    /// 观察着工具类
    /// </summary>
    internal static class UIObserverUtils
    {
        private static Dictionary<Type, IUIObserverOperate> uiObserverOperateDic = new Dictionary<Type, IUIObserverOperate>();

        private static Dictionary<Type, TypeConversion> _typeConversionDic = new Dictionary<Type, TypeConversion>();

        static UIObserverUtils()
        {
            Init();
        }
        static void Init()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsClass || type.IsInterface || type.IsEnum || type.IsArray)
                    {
                        continue;
                    }
                    MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    foreach (var method in methods)
                    {
                        if (method.GetCustomAttribute<TypeConversionAttribute>() == null)
                        {
                            continue;
                        }
                        ParameterInfo[] @params = method.GetParameters();
                        if (@params.Length > 1)
                        {
                            continue;
                        }
                        ParameterInfo parameter = @params[0];
                        if (parameter.ParameterType == method.ReturnType)
                        {
                            continue;
                        }
                        TypeConversion conversion = null;
                        if (!_typeConversionDic.TryGetValue(method.ReturnType, out conversion))
                        {
                            conversion = new TypeConversion(method.ReturnType);
                            _typeConversionDic.Add(method.ReturnType, conversion);
                        }
                        conversion.AddConversion(parameter.ParameterType, method);
                    }
                }
            }
        }
        /// <summary>
        /// 获取一个类型转换的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static Func<TResult, TTarget> GetTypeConversion<TResult, TTarget>()
        {
            if (_typeConversionDic.TryGetValue(typeof(TTarget), out TypeConversion conversion))
            {
                return conversion.GetConversion(typeof(TResult)) as Func<TResult, TTarget>;
            }
            return null;
        }
        /// <summary>
        /// 获取一个订阅者
        /// </summary>
        /// <returns></returns>
        public static IUIObserverOperate GetSubscriber<TResult>()
        {
            Type type = typeof(TResult);
            if (uiObserverOperateDic.ContainsKey(type))
            {
                return uiObserverOperateDic[type];
            }
            UIObserverOperate<TResult> operate = new UIObserverOperate<TResult>();
            uiObserverOperateDic.Add(type, operate);
            return operate;
        }

        /// <summary>
        /// 获取一个绑定类型中的值
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="bindable"></param>
        /// <param name="subscribeKey"></param>
        /// <returns></returns>
        public static TResult GetValue<TKey, TResult>(IBindable ibindable, TKey subscribeKey)
        {
            if (ibindable.GetHandle(subscribeKey) is BindableHandle<TResult> handle)
                return handle.GetValue();
            return default;
        }
        /// <summary>
        /// 给绑定类型设置一个值
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="bindable"></param>
        /// <param name="subscribeKey"></param>
        /// <param name="value"></param>
        public static void SetValue<TKey, TResult>(IBindable ibindable, TKey subscribeKey, TResult value)
        {
            if (ibindable.GetHandle(subscribeKey) is BindableHandle<TResult> handle)
                handle.SetValue(value);
        }
    }
}
