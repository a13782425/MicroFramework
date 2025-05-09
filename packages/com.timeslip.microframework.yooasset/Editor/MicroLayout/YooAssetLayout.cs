﻿using MFramework.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace MFramework.Editor
{
    internal class YooAssetLayout : BaseMicroLayout
    {
        public override string Title => "资源管理/YooAsset";
        //private MarkdownElement _markdownElement;
        public override bool Init()
        {
            MarkdownViewer _markdownElement = new MarkdownViewer();
            panel.Add(_markdownElement);
            _markdownElement.SetMarkdown(_markdown);
            _markdownElement.style.flexGrow = 9;
            _markdownElement.style.flexBasis = 1;
            Button button = new Button(() => { Application.OpenURL("https://www.yooasset.com/"); });
            button.text = "前往主页";
            button.style.height = 64;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.textShadow = new TextShadow() { blurRadius = 1, offset = new Vector2(2, 2), color = Color.black };
            button.style.flexGrow = 1;
            button.style.flexBasis = 1;
            button.style.fontSize = 36;
            panel.Add(button);
            panel.style.marginTop = 6;
            return true;
        }

        private readonly string _markdown = @"# YooAsset

**YooAsset**是一套用于Unity3D的资源管理系统，用于帮助研发团队快速部署和交付游戏。

它可以满足商业化游戏的各类需求，并且经历多款百万DAU游戏产品的验证。

**YooAsset可以满足以下任何需求：**

- 我想发布一个不包含任何游戏资源的安装包，然后玩家边玩边下载。
- 我想发布一个可以保证前期体验的安装包，然后玩家自己选择下载关卡内容。
- 我想发布一个保证300MB以下内容的安装包，然后进入游戏之前把剩余内容下载完毕。
- 我想发布一个偏单机的游戏安装包，在网络畅通的时候，支持正常更新。在没有网络的时候，支持游玩老版本。
- 我想发布一个MOD游戏安装包，玩家可以把自己制作的MOD内容上传到服务器，其它玩家可以下载游玩。
- 我们在制作一个超大体量的项目，有上百GB的资源内容，每次构建都花费大量时间，是否可以分工程构建？

### 系统特点

- **构建管线无缝切换**

  支持传统的内置构建管线，也支持可编程构建管线（SBP）。

- **支持分布式构建**

  支持分工程构建，支持工程里分内容构建，很方便支持游戏模组（MOD）。

- **支持可寻址资源定位**

  默认支持完整路径的资源定位，也支持可寻址资源定位，不需要繁琐的过程即可高效的配置寻址路径。

- **安全高效的分包方案**

  基于资源标签的分包方案，自动对依赖资源包进行分类，避免人工维护成本。可以非常方便的实现零资源安装包，或者全量资源安装包。

- **强大灵活的打包系统**

  可以自定义打包策略，自动分析依赖实现资源零冗余，基于资源对象的资源包依赖管理方案，天然的避免了资源包之间循环依赖的问题。

- **基于引用计数方案**

  基于引用计数的管理方案，可以帮助我们实现安全的资源卸载策略，更好的对内存管理，避免资源对象冗余。还有强大的分析器可帮助发现潜在的资源泄漏问题。

- **多种模式自由切换**

  编辑器模拟模式，单机运行模式，联机运行模式，WebGL运行模式。在编辑器模拟模式下，可以不构建资源包来模拟真实环境，在不修改任何代码的情况下，可以自由切换到其它模式。

- **强大安全的加载系统**

  - **异步加载** 支持协程，Task，委托等多种异步加载方式。
  - **同步加载** 支持同步加载和异步加载混合使用。
  - **边玩边下载** 在加载资源对象的时候，如果资源对象依赖的资源包在本地不存在，会自动从服务器下载到本地，然后再加载资源对象。
  - **多线程下载** 支持断点续传，自动验证下载文件，自动修复损坏文件。
  - **多功能下载器** 可以按照资源分类标签创建下载器，也可以按照资源对象创建下载器。可以设置同时下载文件数的限制，设置下载失败重试次数，设置下载超时判定时间。多个下载器同时下载不用担心文件重复下载问题，下载器还提供了下载进度以及下载失败等常用接口。

- **原生格式文件管理**

  无缝衔接资源打包系统，可以很方便的实现原生文件的版本管理和下载。

- **灵活多变的版本管理**

  支持线上版本快速回退，支持区分审核版本，测试版本，线上版本，支持灰度更新及测试。

- **多平台的完美适配**

  支持安卓，苹果，PC等常规平台，支持网页运行。2.x版本完美适配了微信小游戏平台和抖音小游戏平台。

### 官方主页（教程文档）

https://www.yooasset.com/";
    }
}
