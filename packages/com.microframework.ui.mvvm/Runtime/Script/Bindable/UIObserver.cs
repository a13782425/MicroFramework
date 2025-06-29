using MFramework.Core;
using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace MFramework.Runtime
{
    public interface IUIObserver
    {
        BindingMode mode => BindingMode.OneWay;
        /// <summary>
        /// 建立观察
        /// </summary>
        void Build();
        /// <summary>
        /// 取消观察
        /// </summary>
        void UnBuild();
    }
    internal interface IUIObserverOperate
    {
        void Subscribe(IBindable bindable, string memberName, object action, bool firstNotify) { }
        void UnSubscribe(IBindable bindable, string memberName, object action) { }
        void SetValue(IBindable bindable, string memberName, object value) { }
        object GetValue(IBindable bindable, string memberName) => default;
        Type GetResultType() => default;
    }

    internal class UIObserverOperate<TResult> : IUIObserverOperate
    {
        private IMicroLogger logger = MicroLogger.GetMicroLogger("UIObserver");
        void IUIObserverOperate.Subscribe(IBindable bindable, string memberName, object action, bool firstNotify)
        {
            bindable.Subscribe(memberName, (BindableDelegate<TResult>)action, firstNotify);
        }
        void IUIObserverOperate.UnSubscribe(IBindable bindable, string memberName, object action)
        {
            bindable.Unsubscribe(memberName, (BindableDelegate<TResult>)action);
        }
        void IUIObserverOperate.SetValue(IBindable bindable, string memberName, object value)
        {
            if (value is TResult result)
            {
                UIObserverUtils.SetValue<string, TResult>(bindable, memberName, result);
            }
            else
            {
                logger.LogError($"SetValue值类型错误,目标类型为:{typeof(TResult).Name}");
            }
        }
        object IUIObserverOperate.GetValue(IBindable bindable, string memberName)
        {
            return UIObserverUtils.GetValue<string, TResult>(bindable, memberName);
        }
        Type IUIObserverOperate.GetResultType() => typeof(TResult);
    }
    /// <summary>
    /// UI观察着
    /// <para>用于关联界面和Bindable</para>
    /// </summary>
    public sealed partial class UIObserver<TComponent, TModel> : IUIObserver
        where TComponent : IBindable
        where TModel : IBindable
    {
        public readonly TComponent component;
        public readonly TModel model;

        private string _comMemberName;
        /// <summary>
        /// Action<BindableEventArgs<TResult>>
        /// </summary>
        private object _comAction;
        private Func<object, object> _comFormatFunc;
        private IUIObserverOperate _comOperate;

        private string _modelMemberName;
        /// <summary>
        /// Action<BindableEventArgs<TResult>>
        /// </summary>
        private object _modelAction;
        private Func<object, object> _modelFormatFunc;
        private IUIObserverOperate _modelOperate;
        private bool _useTypeConversion = false;
        private BindingMode _mode = BindingMode.OneWay;
        BindingMode IUIObserver.mode => _mode;
        /// <summary>
        /// 是否已经构建
        /// </summary>
        private bool _isBuild = false;
        internal UIObserver(TComponent component, TModel model)
        {
            this.component = component;
            this.model = model;
        }

        public UIObserver<TComponent, TModel> For<TResult>(Expression<Func<TComponent, TResult>> expression)
        {
            Debug.Assert(!_isBuild, "已经构建完成,无法修改,如需修改请先取消构建");
            this._comMemberName = BindableUtils.ParseMemberName(expression);
            this._comOperate = UIObserverUtils.GetSubscriber<TResult>();
            return this;
        }

        public UIObserver<TComponent, TModel> To<TResult>(Expression<Func<TModel, TResult>> expression)
        {
            Debug.Assert(!_isBuild, "已经构建完成,无法修改,如需修改请先取消构建");
            this._modelMemberName = BindableUtils.ParseMemberName(expression);
            this._modelOperate = UIObserverUtils.GetSubscriber<TResult>();
            return this;
        }
        public UIObserver<TComponent, TModel> ForFormat(Func<object, object> func)
        {
            Debug.Assert(!_isBuild, "已经构建完成,无法修改,如需修改请先取消构建");
            _comFormatFunc = func;
            return this;
        }
        public UIObserver<TComponent, TModel> ToFormat(Func<object, object> func)
        {
            Debug.Assert(!_isBuild, "已经构建完成,无法修改,如需修改请先取消构建");
            _modelFormatFunc = func;
            return this;
        }
        public UIObserver<TComponent, TModel> TwoWay()
        {
            Debug.Assert(!_isBuild, "已经构建完成,无法修改,如需修改请先取消构建");
            _mode = BindingMode.TwoWay;
            return this;
        }
        public UIObserver<TComponent, TModel> OneWay()
        {
            Debug.Assert(!_isBuild, "已经构建完成,无法修改,如需修改请先取消构建");
            _mode = BindingMode.OneWay;
            return this;
        }
        public UIObserver<TComponent, TModel> OneWayToSource()
        {
            Debug.Assert(!_isBuild, "已经构建完成,无法修改,如需修改请先取消构建");
            _mode = BindingMode.OneWayToSource;
            return this;
        }
        /// <summary>
        /// 是否使用类型转换
        /// 如果使用它会在Format方法前先调用
        /// </summary>
        /// <param name="isUse"></param>
        /// <returns></returns>
        public UIObserver<TComponent, TModel> UseTypeConversion(bool isUse = true)
        {
            _useTypeConversion = isUse;
            return this;
        }
        public void Build() => Build(true);
        public void Build(bool firstNotify)
        {
            Debug.Assert(!_isBuild, "已经构建完成,无法修改,如需修改请先取消构建");
            _isBuild = true;
            if (this._modelOperate != null && !string.IsNullOrWhiteSpace(this._modelMemberName))
            {
                this._modelAction = m_genChangedAction(this._modelOperate.GetResultType(), true);
                this._modelOperate.Subscribe(this.model, this._modelMemberName, this._modelAction, firstNotify);
            }
            if (this._comOperate != null && !string.IsNullOrWhiteSpace(this._comMemberName))
            {
                this._comAction = m_genChangedAction(this._comOperate.GetResultType());
                this._comOperate.Subscribe(this.component, this._comMemberName, this._comAction, firstNotify);
            }
        }

        public void UnBuild()
        {
            Debug.Assert(_isBuild, "尚未构建,无法取消构建");
            if (this._modelOperate != null && !string.IsNullOrWhiteSpace(this._modelMemberName))
            {
                this._modelOperate.UnSubscribe(this.model, this._modelMemberName, this._modelAction);
                this._modelAction = null;
            }
            if (this._comOperate != null && !string.IsNullOrWhiteSpace(this._comMemberName))
            {
                this._comOperate.UnSubscribe(this.component, this._comMemberName, this._comAction);
                this._comAction = null;
            }
            _isBuild = false;
        }
        /// <summary>
        /// 生成修改的方法
        /// Action<BindableEventArgs<TResult>>
        /// </summary>
        private object m_genChangedAction(Type resultType, bool isModel = false)
        {
            //void modelChange(BindableEventArgs<TResult> obj)
            //{
            //    if (m_checkMode(BindingMode.OneWay | BindingMode.TwoWay))
            //        return;
            //    if (_modelFormatFunc != null)
            //        this._comOperate.SetValue(this.component, this._comMemberName, _modelFormatFunc.Invoke(obj.newValue));
            //    else
            //        component.SetValue(_comMemberName, obj.newValue);
            //}
            //void comChange(BindableEventArgs<TResult> obj)
            //{
            //    if (m_checkMode(BindingMode.OneWayToSource | BindingMode.TwoWay))
            //        return;
            //    if (_comFormatFunc != null)
            //        this._modelOperate.SetValue(this.model, this._modelMemberName, _comFormatFunc.Invoke(obj.newValue));
            //    else
            //        model.SetValue(_modelMemberName, obj.newValue);
            //}

            BindingFlags noPublic = BindingFlags.NonPublic | BindingFlags.Instance;
            Type type = this.GetType();
            Type paramType = typeof(BindableEventArgs<>).MakeGenericType(resultType);//参数类型
            Type targetType = isModel ? _comOperate.GetResultType() : _modelOperate.GetResultType();//目标类型
            ParameterExpression param = Expression.Parameter(paramType, "obj");
            ConstantExpression @thisConstant = Expression.Constant(this, type);
            MemberExpression modeField = Expression.Field(@thisConstant, type.GetField("_mode", noPublic));

            MemberExpression formatFuncField = Expression.Field(@thisConstant, type.GetField(isModel ? "_modelFormatFunc" : "_comFormatFunc", noPublic));
            MemberExpression operateField = Expression.Field(@thisConstant, type.GetField(isModel ? "_comOperate" : "_modelOperate", noPublic));
            MemberExpression memberNameField = Expression.Field(@thisConstant, type.GetField(isModel ? "_comMemberName" : "_modelMemberName", noPublic));
            MemberExpression bindableField = Expression.Field(@thisConstant, type.GetField(isModel ? "component" : "model"));
            ConstantExpression modeConstant = Expression.Constant(isModel ? (int)(BindingMode.OneWay | BindingMode.TwoWay) : (int)(BindingMode.OneWayToSource | BindingMode.TwoWay), typeof(int));

            LabelTarget returnLabel = Expression.Label("ret");
            //检查绑定模式
            var checkModeExpr = Expression.IfThen(
                Expression.Equal(
                    Expression.And(
                        Expression.Convert(modeField, typeof(int)),
                        modeConstant),
                    Expression.Constant(0, typeof(int))
                    ),
                Expression.Return(returnLabel));

            LambdaExpression lambda = null;
            ConditionalExpression callSetValueExpr = null;
            if (this._useTypeConversion && resultType != targetType)
            {
                //如果需要类型转换且目标类型和结果类型不一致

                //获取类型转换
                MethodInfo getConversionMethod = typeof(UIObserverUtils).GetMethod("GetTypeConversion").MakeGenericMethod(resultType, targetType);
                Delegate conversionMethod = (Delegate)getConversionMethod.Invoke(null, null);
                if (conversionMethod == null)
                {
                    //如果转换方法为Null,
                    goto NoConversion;
                }
                else
                {
                    ConstantExpression conversionExpr = Expression.Constant(conversionMethod);
                    ParameterExpression targetVar = Expression.Variable(targetType, "target");
                    BinaryExpression targetAssign = Expression.Assign(targetVar, Expression.Invoke(conversionExpr, Expression.Property(param, "newValue")));
                    //调用SetValue 
                    callSetValueExpr = Expression.IfThenElse(
                        Expression.NotEqual(
                            formatFuncField,
                            Expression.Default(typeof(object))),
                        Expression.Call(
                            operateField,
                            typeof(IUIObserverOperate).GetMethod("SetValue"),
                            bindableField,
                            memberNameField,
                            Expression.Invoke(
                                formatFuncField,
                                targetType.IsClass ? targetVar : Expression.Convert(targetVar, typeof(object))
                                )),
                        Expression.Call(
                            typeof(UIObserverUtils).GetMethod("SetValue").MakeGenericMethod(typeof(string), targetType),
                            bindableField,
                            memberNameField,
                            targetVar
                            )
                        );
                    lambda = Expression.Lambda(typeof(BindableDelegate<>).MakeGenericType(resultType), Expression.Block(new[] { targetVar }, checkModeExpr, targetAssign, callSetValueExpr, Expression.Label(returnLabel)), param);
                    goto Compile;
                }
            }

        //调用SetValue 
        NoConversion: callSetValueExpr = Expression.IfThenElse(
            Expression.NotEqual(
                formatFuncField,
                Expression.Default(typeof(object))),
            Expression.Call(
                operateField,
                typeof(IUIObserverOperate).GetMethod("SetValue"),
                bindableField,
                memberNameField,
                Expression.Invoke(
                    formatFuncField,
                    resultType.IsClass ? Expression.Property(param, "newValue") : Expression.Convert(Expression.Property(param, "newValue"), typeof(object))
                    )),
            Expression.Call(
                typeof(UIObserverUtils).GetMethod("SetValue").MakeGenericMethod(typeof(string), resultType),
                bindableField,
                memberNameField,
                Expression.Property(param, "newValue")
                )
            );
            lambda = Expression.Lambda(typeof(BindableDelegate<>).MakeGenericType(resultType), Expression.Block(checkModeExpr, callSetValueExpr, Expression.Label(returnLabel)), param);

        Compile: object func = lambda.Compile();
            return func;
        }


    }
}
