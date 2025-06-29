using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    /// <summary>
    /// 编辑器布局
    /// </summary>
    public abstract class BaseMicroLayout : IConstructor
    {
        /// <summary>
        /// 当前所属的窗口
        /// </summary>
        internal protected EditorWindow window { get; internal set; }

        /// <summary>
        /// 当前布局的界面
        /// </summary>
        internal protected VisualElement panel { get; internal set; }
        /// <summary>
        /// 标题,以斜杠区分层级
        /// <para>XX/XX</para>
        /// </summary>
        public abstract string Title { get; }
        /// <summary>
        /// 排序
        /// <para>从小到大排序</para>
        /// </summary>
        public virtual int Priority => 0;
        /// <summary>
        /// 初始化
        /// 用于初始化一些数据及界面
        /// </summary>
        /// <returns>是否初始化成功</returns>
        public virtual bool Init() => true;

        /// <summary>
        /// Title被点击时候显示的界面
        /// </summary>        
        public virtual void ShowUI() { }
        /// <summary>
        /// 运行时配置改变时
        /// </summary>
        public virtual void OnRuntimeConfigChanged() { }

        /// <summary>
        /// 更新在当前界面处于显示状态时
        /// </summary>
        public virtual void OnUpdate() { }
        /// <summary>
        /// 隐藏界面
        /// </summary>
        public virtual void HideUI() { }

        /// <summary>
        /// 退出
        /// 在界面被关闭时
        /// </summary>
        public virtual void Exit() { }

        protected void Add(VisualElement element) => panel.Add(element);
    }
}
