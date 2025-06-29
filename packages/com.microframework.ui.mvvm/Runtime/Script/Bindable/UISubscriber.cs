//using MFramework.Core;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Net.NetworkInformation;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace MFramework.Runtime
//{
//    internal interface ISubscriber
//    {
//        BindingMode mode => BindingMode.OneWay;
//        void Build() { }
//    }
//    internal interface ISubscriberAtom
//    {
//        void Subscribe(IBindable bindable, string memberName, object action) { }
//        void UnSubscribe(IBindable bindable, string memberName, object action) { }
//        void SetValue(IBindable bindable, string memberName, object value) { }
//        object GetValue(IBindable bindable, string memberName) => default;
//        Type GetResultType() => default;
//    }

//    internal class SubscriberAtom<TResult> : ISubscriberAtom
//    {
//        void ISubscriberAtom.Subscribe(IBindable bindable, string memberName, object action)
//        {
//            bindable.Subscribe(memberName, (Action<BindableEventArgs<TResult>>)action);
//        }
//        void ISubscriberAtom.UnSubscribe(IBindable bindable, string memberName, object action)
//        {
//            bindable.Unsubscribe(memberName, (Action<BindableEventArgs<TResult>>)action);
//        }
//        void ISubscriberAtom.SetValue(IBindable bindable, string memberName, object value)
//        {

//        }
//        object ISubscriberAtom.GetValue(IBindable bindable, string memberName)
//        {
//            return bindable.GetValue<string, TResult>(memberName);
//        }
//        Type ISubscriberAtom.GetResultType() => typeof(TResult);
//    }
//    /// <summary>
//    /// UI订阅者
//    /// </summary>
//    public sealed class UISubscriber<TComponent, TModel> : ISubscriber
//        where TComponent : IBindable
//        where TModel : IBindable
//    {
//        public readonly TComponent component;
//        public readonly TModel model;
//        private string _comMemberName;
//        private string _modelMemberName;
//        private Type _comResultType;
//        private Type _modelResultType;
//        private ISubscriberAtom _comAtom;
//        private ISubscriberAtom _modelAtom;
//        /// <summary>
//        /// Action<BindableEventArgs<TResult>>
//        /// </summary>
//        private object _comAction;
//        private Func<object, object> _comFormatFunc;
//        /// <summary>
//        /// Action<BindableEventArgs<TResult>>
//        /// </summary>
//        private object _modelAction;
//        private Func<object, object> _modelFormatFunc;

//        private BindingMode _mode = BindingMode.OneWay;
//        BindingMode ISubscriber.mode => _mode;
//        internal UISubscriber(TComponent component, TModel model)
//        {
//            this.component = component;
//            this.model = model;
//        }
//        public UISubscriber<TComponent, TModel> For<TResult>(Expression<Func<TComponent, TResult>> expression)
//        {
//            if (_comAction != null)
//            {
//                component.Unsubscribe(_comMemberName, _comAction as Action<BindableEventArgs<TResult>>);
//                _comAction = null;
//            }
//            _comResultType = typeof(TResult);
//            _comMemberName = BindableUtils.ParseMemberName(expression);
//            var action = m_genModelChangedAction<TResult>();
//            _comAction = action;
//            component.Subscribe(_comMemberName, action);
//            return this;
//        }
//        public UISubscriber<TComponent, TModel> To<TResult>(Expression<Func<TModel, TResult>> expression)
//        {
//            if (_modelAction != null)
//            {
//                model.Unsubscribe(_modelMemberName, _modelAction as Action<BindableEventArgs<TResult>>);
//                _modelAction = null;
//            }
//            _modelResultType = typeof(TResult);
//            _modelMemberName = BindableUtils.ParseMemberName(expression);
//            var action = m_genModelChangedAction<TResult>(true);
//            _modelAction = action;
//            model.Subscribe(_modelMemberName, action);
//            return this;
//        }

//        public UISubscriber<TComponent, TModel> ToFormat(Func<object, object> func)
//        {
//            _comFormatFunc = func;
//            return this;
//        }
//        public UISubscriber<TComponent, TModel> ForFormat(Func<object, object> func)
//        {
//            _modelFormatFunc = func;
//            return this;
//        }
//        public UISubscriber<TComponent, TModel> TwoWay()
//        {
//            _mode = BindingMode.TwoWay;
//            return this;
//        }
//        public UISubscriber<TComponent, TModel> OneWay()
//        {
//            _mode = BindingMode.OneWay;
//            return this;
//        }
//        public UISubscriber<TComponent, TModel> OneWayToSource()
//        {
//            _mode = BindingMode.OneWayToSource;
//            return this;
//        }
//        /// <summary>
//        /// 生成修改的方法
//        /// Action<BindableEventArgs<TResult>>
//        /// </summary>
//        private Action<BindableEventArgs<TResult>> m_genModelChangedAction<TResult>(bool isModel = false)
//        {
//            /*
//                     private void m_testModelChanged(BindableEventArgs<string> obj)
//                    {
//                        if (_mode == BindingMode.TwoWay || _mode == BindingMode.OneWay)
//                        {
//                            string newValue = "";
//                            if (_modelFormatFunc != null)
//                            {
//                                object temp = _modelFormatFunc?.Invoke(obj.newValue);
//                                if (temp.GetType() == typeof(string))
//                                {
//                                    newValue = temp.ToString();
//                                }
//                                else
//                                {
//                                    Debug.LogError("Format函数返回值类型错误,使用原值");
//                                    newValue = obj.newValue;
//                                }
//                            }
//                            component.SetValue(_modelMemberName, newValue);
//                        }
//                    }
//             */
//            BindingFlags noPublic = BindingFlags.NonPublic | BindingFlags.Instance;
//            Type resultType = typeof(TResult);
//            Type type = this.GetType();
//            MethodInfo getTypeMethod = typeof(object).GetMethod("GetType");
//            ParameterExpression param = Expression.Parameter(typeof(BindableEventArgs<>).MakeGenericType(resultType), "obj");
//            ConstantExpression @thisConstant = Expression.Constant(this, type);
//            ParameterExpression valueVar = Expression.Variable(resultType, "value1");
//            ParameterExpression tempVar = Expression.Variable(typeof(object), "tempValue");

//            MemberExpression formatFuncField = Expression.Field(@thisConstant, type.GetField("_comFormatFunc", noPublic));
//            MemberExpression memberNameField = Expression.Field(@thisConstant, type.GetField("_modelMemberName", noPublic));
//            MemberExpression bindableField = Expression.Field(@thisConstant, type.GetField("model"));
//            ConstantExpression modeConstant = Expression.Constant(BindingMode.OneWayToSource | BindingMode.TwoWay, typeof(BindingMode));
//            if (isModel)
//            {
//                formatFuncField = Expression.Field(@thisConstant, type.GetField("_modelFormatFunc", noPublic));
//                memberNameField = Expression.Field(@thisConstant, type.GetField("_comMemberName", noPublic));
//                bindableField = Expression.Field(@thisConstant, type.GetField("component"));
//                modeConstant = Expression.Constant(BindingMode.OneWay | BindingMode.TwoWay, typeof(BindingMode));
//            }


//            var resultTypeExpr = Expression.Constant(resultType, typeof(Type));
//            var errorMessage = Expression.Constant($"Format函数返回值类型错误,已使用原值", typeof(string));
//            //做最后的数据检测
//            var valueCheckExpr = Expression.IfThenElse(
//                Expression.AndAlso(Expression.NotEqual(tempVar, Expression.Default(typeof(object))), Expression.Equal(Expression.Call(tempVar, getTypeMethod), resultTypeExpr)),
//                Expression.Assign(valueVar, resultType.IsClass ? Expression.TypeAs(tempVar, resultType) : Expression.Convert(tempVar, resultType)),
//                Expression.Call(typeof(UnityEngine.Debug).GetMethod("LogError", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(object) }, null), errorMessage)
//                );
//            //检测format是否可以用
//            var funcCheckExpr = Expression.IfThen(
//               Expression.NotEqual(formatFuncField, Expression.Default(typeof(Func<object, object>))),
//               Expression.Block(
//                   new[] { tempVar },
//                   Expression.Assign(tempVar, Expression.Invoke(formatFuncField, Expression.Property(param, "newValue"))),
//                   Expression.Call(typeof(UnityEngine.Debug).GetMethod("LogError", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(object) }, null), tempVar),
//                   valueCheckExpr)
//               );

//            //获取参数中的newValue
//            var assignValue = Expression.Assign(valueVar, Expression.Property(param, "newValue"));
//            //调用设置值方法
//            var callSetValueExpr = Expression.Call(typeof(BindableUtils).GetMethod("SetValue").MakeGenericMethod(memberNameField.Type, valueVar.Type), bindableField, memberNameField, valueVar);

//            var modeCheckExpr = Expression.IfThen(
//                Expression.NotEqual(
//                    Expression.And(Expression.Convert(Expression.Field(thisConstant, this.GetType().GetField("_mode", noPublic)), typeof(int)), Expression.Convert(modeConstant, typeof(int))),
//                    Expression.Constant(0, typeof(int))),
//                Expression.Block(new[] { valueVar }, assignValue, funcCheckExpr, callSetValueExpr)
//                );
//            var lambda = Expression.Lambda<Action<BindableEventArgs<TResult>>>(modeCheckExpr, param);
//            Action<BindableEventArgs<TResult>> action = lambda.Compile();

//            return action;
//        }
//        private void m_testModelChanged(BindableEventArgs<string> obj)
//        {
//            if (_mode == BindingMode.TwoWay || _mode == BindingMode.OneWay)
//            {
//                string newValue = obj.newValue;
//                if (_modelFormatFunc != null)
//                {
//                    object temp = _modelFormatFunc?.Invoke(obj.newValue);
//                    if (temp != null && temp.GetType() == typeof(string))
//                    {
//                        newValue = temp.ToString();
//                    }
//                    else
//                    {
//                        Debug.LogError("Format函数返回值类型错误,使用原值");
//                    }
//                }
//                component.SetValue(_modelMemberName, newValue);
//            }
//        }
//        private void m_testModelChanged1(BindableEventArgs<string> obj)
//        {
//            var tempMode = BindingMode.OneWayToSource | BindingMode.TwoWay;
//            if ((tempMode & _mode) == 0)
//            {
//                return;
//            }

//            if (_mode == BindingMode.TwoWay || _mode == BindingMode.OneWay)
//            {
//                string newValue = obj.newValue;
//                if (_modelFormatFunc != null)
//                {
//                    object temp = _modelFormatFunc?.Invoke(obj.newValue);
//                    if (temp != null && temp.GetType() == typeof(string))
//                    {
//                        newValue = temp.ToString();
//                    }
//                    else
//                    {
//                        Debug.LogError("Format函数返回值类型错误,使用原值");
//                    }
//                }
//                component.SetValue(_modelMemberName, newValue);
//            }
//        }
//    }
//}
