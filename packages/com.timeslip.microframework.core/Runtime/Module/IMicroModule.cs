using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFramework.Core
{
    /// <summary>
    /// 模块的状态
    /// </summary>
    public enum ModuleState
    {
        None,
        /// <summary>
        /// 初始化
        /// </summary>
        Initializing,
        /// <summary>
        /// 运行
        /// </summary>
        Running,
        /// <summary>
        /// 中止
        /// </summary>
        Suspended,
        /// <summary>
        /// 释放
        /// </summary>
        Destory,
    }

    /// <summary>
    /// 模块更新接口
    /// </summary>
    public interface IMicroUpdate
    {
        /// <summary>
        /// 更新接口
        /// </summary>
        /// <param name="deltaTime"></param>
        void OnUpdate(float deltaTime);
    }
    /// <summary>
    /// 模块更新接口
    /// </summary>
    public interface IMicroLogicUpdate
    {
        /// <summary>
        /// 逻辑帧帧率
        /// <para>如果<=0,则用系统逻辑帧率</para>
        /// <para>如果跟新频率过高则会导致游戏卡顿</para>
        /// </summary>
        int LogicFrame { get; set; }
        /// <summary>
        /// 逻辑更新接口
        /// </summary>
        /// <param name="deltaTime"></param>
        void OnLogicUpdate(float deltaTime);
    }
    /// <summary>
    /// 模块接口
    /// </summary>
    public interface IMicroModule
    {
        /// <summary>
        /// 是否初始化完成
        /// </summary>
        bool IsInit { get; }
        /// <summary>
        /// 初始化
        /// </summary>
        void OnInit();
        /// <summary>
        /// 暂停使用
        /// <para>在初始化完成后删除了依赖,会调用此方法</para>
        /// </summary>
        void OnSuspend();
        /// <summary>
        /// 恢复使用
        /// <para>在初始化完成后恢复其依赖关系,会调用此方法</para>
        /// </summary>
        void OnResume();
        /// <summary>
        /// 释放
        /// </summary>
        void OnDestroy();
    }

}
