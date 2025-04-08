using MFramework.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace MFramework.Runtime
{
    /// <summary>
    /// 资源加载回调
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="obj">加载完成资源</param>
    /// <param name="args">自定义参数</param>
    public delegate void ResLoadDelegate<T>(T obj, object args) where T : UnityObject;
    /// <summary>
    /// 资源模块接口
    /// </summary>
    public interface IResourceModule : IMicroModule
    {
        /// <summary>
        /// 同步加载一个资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resPath"></param>
        /// <returns></returns>
        T LoadSync<T>(string resPath) where T : UnityObject;
        /// <summary>
        /// 同步加载一个资源
        /// </summary>
        /// <param name="resPath"></param>
        /// <param name="resType"></param>
        /// <returns></returns>
        UnityObject LoadSync(string resPath, Type resType);
        /// <summary>
        /// 异步加载一个资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resPath"></param>
        /// <param name="callback">加载完成后回调</param>
        /// <param name="arg">自定义参数</param>
        /// <returns></returns>
        ResHandler Load<T>(string resPath, ResLoadDelegate<T> callback, object arg = null) where T : UnityObject;
        /// <summary>
        /// 异步加载一个资源
        /// </summary>
        /// <param name="resPath"></param>
        /// <param name="resType"></param>
        /// <param name="callback">加载完成后回调</param>
        /// <param name="arg">自定义参数</param>
        /// <returns></returns>
        ResHandler Load(string resPath, Type resType, ResLoadDelegate<UnityObject> callback, object arg = null);
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="resPath">资源路径</param>
        /// <param name="reference">释放的引用次数</param>
        void UnloadAsset(string resPath, int reference = 1);
    }
}
