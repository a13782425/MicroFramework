using MFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.UI
{
    /// <summary>
    /// UI模块接口
    /// </summary>
    public interface IUIModule : IMicroModule
    {
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
        /// 判断某个类型的Panel是否显示
        /// <para>如果该类型界面不存在，则为false</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool IsOpen<T>() where T : UIPanel, new();
        /// <summary>
        /// 获得一个界面，如果没有则创建
        /// <para>不会改变界面的启用状态</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetPanel<T>() where T : UIPanel, new();
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
    }
}
