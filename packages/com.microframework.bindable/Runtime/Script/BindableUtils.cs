using MFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace MFramework.Runtime
{

    /// <summary>
    /// 数据绑定工具类
    /// </summary>
    public static class BindableUtils
    {
        /// <summary>
        /// 对比方法缓存
        /// 需要对比的类型
        /// <para>Func<T,T,bool></para>
        /// </summary>
        private static Dictionary<Type, object> _equalsCacheDic = new Dictionary<Type, object>();

        private static Dictionary<Type, Func<object, Dictionary<string, BindableHandle>>> _getHandleCacheDic = new Dictionary<Type, Func<object, Dictionary<string, BindableHandle>>>();

        public const string BINDABLE_ALL_KEY = "*";

        /// <summary>
        /// 判断两个值是否相等
        /// </summary>
        /// <typeparam name="T">任意类型</typeparam>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static bool EqualValue<T>(T t1, T t2)
        {
            return GetEqualValue<T>()(t1, t2);
        }
        /// <summary>
        /// 判断两个值是否相等
        /// </summary>
        /// <typeparam name="T">任意类型</typeparam>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static Func<T, T, bool> GetEqualValue<T>()
        {
            return EqualityComparer<T>.Default.Equals;
        }

        /// <summary>
        /// 获取数据驱动类的Handle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bindable"></param>
        /// <param name="subscribeKey"></param>
        /// <returns></returns>
        public static BindableHandle<T> GetHandle<T>(IBindable bindable, string subscribeKey)
        {
            var handles = s_getHandles(bindable).Invoke(bindable);
            if (!handles.TryGetValue(subscribeKey, out BindableHandle handle))
            {
                if (!handles.TryGetValue(BINDABLE_ALL_KEY, out BindableHandle parent))
                {
                    parent = s_createHandle(bindable, BINDABLE_ALL_KEY, null);
                    handles.Add(BINDABLE_ALL_KEY, parent);
                }

                handle = s_createHandle<T>(bindable, subscribeKey, parent);
                handles.Add(subscribeKey, handle);
            }
            return handle as BindableHandle<T>;
        }
        /// <summary>
        /// 获取Lambda表达式中的字段名
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string ParseMemberName<TBind, TResult>(Expression<Func<TBind, TResult>> expression)
        {
            if (expression == null)
                throw new System.ArgumentNullException("expression");

            var body = expression.Body as MemberExpression;
            if (body == null)
            {
                MicroLogger.LogError(string.Format("Invalid expression:{0}", expression));
                return null;
            }
            if (!(body.Expression is ParameterExpression))
            {
                MicroLogger.LogError(string.Format("Invalid expression:{0}", expression));
                return null;
            }
            return body.Member.Name;
        }
        /// <summary>
        /// 获取Lambda表达式中的字段名
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string ParseMemberName(LambdaExpression expression)
        {
            if (expression == null)
                throw new System.ArgumentNullException("expression");

            var body = expression.Body as MemberExpression;
            if (body == null)
            {
                MicroLogger.LogError(string.Format("Invalid expression:{0}", expression));
                return null;
            }
            if (!(body.Expression is ParameterExpression))
            {
                MicroLogger.LogError(string.Format("Invalid expression:{0}", expression));
                return null;
            }
            return body.Member.Name;
        }
        private static BindableHandle<TResult> s_createHandle<TResult>(object sender, object key, BindableHandle parent)
        {
            BindableHandle<TResult> handle = new BindableHandle<TResult>();
            handle.Init(sender, key, parent);
            return handle;
        }
        private static BindableHandle s_createHandle(object sender, object key, BindableHandle parent)
        {
            BindableHandle handle = new BindableHandle();
            handle.Init(sender, key, null);
            return handle;
        }
        private static Func<object, Dictionary<string, BindableHandle>> s_getHandles(IBindable dataDriven)
        {
            Type type = dataDriven.GetType();
            if (_getHandleCacheDic.TryGetValue(type, out var func))
            {
                return func;
            }
            MethodInfo method = type.GetMethod($"GetHandlers_{type.Name.ToUpper()}", BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
                return null;

            var param_dataDriven = Expression.Parameter(typeof(object), "dd");
            var call_getHandle = Expression.Call(Expression.TypeAs(param_dataDriven, type), method);
            var lambda = Expression.Lambda<Func<object, Dictionary<string, BindableHandle>>>(Expression.Block(call_getHandle), param_dataDriven);
            func = lambda.Compile();
            _getHandleCacheDic.Add(type, func);
            return func;
        }

        #region 使用ExpressionVisitor
#if USE_VISITOR
        private class CustomExpressionVisitor : ExpressionVisitor
        {
            public string memberName;

            protected override Expression VisitMember(MemberExpression node)
            {
                memberName = node.Member.Name;
                return base.VisitMember(node);
            }
        }

        private static CustomExpressionVisitor _visitor = new CustomExpressionVisitor();
        /// <summary>
        /// 获取Lambda表达式中的字段名
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string ParseMemberName(LambdaExpression expression)
        {
            _visitor.Visit(expression);
            return _visitor.memberName;
        } 
#endif
        #endregion
    }
}
