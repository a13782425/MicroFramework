using MFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.UI
{
    public delegate void PanelStateDelegate(UIView view, UIState uIState);
    /// <summary>
    /// UI模块接口
    /// </summary>
    public interface IUIModule : IMicroModule
    {
        /// <summary>
        /// 主Canvas
        /// </summary>
        Canvas MainCanvas { get; }
        /// <summary>
        /// 面板状态发生变化时候调用
        /// </summary>
        event PanelStateDelegate onPanelStateChanged;
        /// <summary>
        /// 创建一个Panel
        /// </summary>
        /// <returns></returns>
        UIPanel CreatePanel(Type panelType, object data = null);
        /// <summary>
        /// 创建一个Panel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T CreatePanel<T>(object data = null) where T : UIPanel, new();
        /// <summary>
        /// 显示一个界面，如果没有则创建并显示
        /// <para>会改变界面的启用状态</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T ShowPanel<T>(object data = null) where T : UIPanel, new();
        /// <summary>
        /// 显示一个界面，如果没有则创建并显示
        /// <para>会改变界面的启用状态</para>
        /// </summary>
        /// <param name="panelType"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        UIPanel ShowPanel(Type panelType, object data = null);
        /// <summary>
        /// 判断某个类型的Panel是否显示
        /// <para>如果该类型界面不存在或者隐藏，则为false</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool IsOpen<T>() where T : UIPanel, new();
        /// <summary>
        /// 判断某个类型的Panel是否显示
        /// <para>如果该类型界面不存在或者隐藏，则为false</para>
        /// </summary>
        /// <param name="type">界面类型</param>
        /// <returns></returns>
        bool IsOpen(Type type);
        /// <summary>
        /// 判断某个类型的Panel是否显示
        /// <para>如果该类型界面不存在或者隐藏，则为false</para>
        /// </summary>
        /// <param name="panelClassName">界面类型</param>
        /// <returns></returns>
        bool IsOpen(string panelClassName);
        /// <summary>
        /// 获得一个界面, 如果没有创建或者被关闭，则返回null
        /// <para>不会改变界面的启用状态</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetPanel<T>() where T : UIPanel, new();
        /// <summary>
        /// 获得一个界面, 如果没有创建或者被关闭，则返回null
        /// <para>不会改变界面的启用状态</para>
        /// </summary>
        /// <param name="panelType"></param>
        /// <returns></returns>
        UIPanel GetPanel(Type panelType);
        /// <summary>
        /// 获得最顶层的界面
        /// 可以获取到当前正在开启的堆栈UI
        /// </summary>
        /// <returns></returns>
        UIPanel GetTopPanel();
        /// <summary>
        /// 隐藏一个界面
        /// </summary>
        /// <param name="panel"></param>
        void HidePanel(UIPanel panel);
        /// <summary>
        /// 隐藏一个界面
        /// </summary>
        void HidePanel<T>() where T : UIPanel, new();
        /// <summary>
        /// 根据层级隐藏界面
        /// </summary>
        /// <param name="layerEnum"></param>
        void HidePanelByLayer(UILayer layerEnum);
        /// <summary>
        /// 隐藏所有界面
        /// </summary>
        void HideAllPanel();
        /// <summary>
        /// 关闭一个界面
        /// </summary>
        /// <param name="panel"></param>
        void ClosePanel(UIPanel panel);
        /// <summary>
        /// 关闭一个界面
        /// </summary>
        void ClosePanel<T>() where T : UIPanel, new();
        /// <summary>
        /// 根据层级关闭界面
        /// </summary>
        /// <param name="layerEnum"></param>
        void ClosePanelByLayer(UILayer layerEnum);
        /// <summary>
        /// 关闭所有界面
        /// </summary>
        void CloseAllPanel();
        /// <summary>
        /// 根据层级获取对应的Transform
        /// </summary>
        /// <returns></returns>
        Transform GetLayer(UILayer layer);
    }
}
