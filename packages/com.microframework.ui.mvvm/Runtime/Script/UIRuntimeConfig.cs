using MFramework.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFramework.Runtime
{
    public sealed class UIRuntimeConfig : ICustomMicroRuntimeConfig
    {
        [DisplayName("设计分辨率:")]
        public Vector2Int DesignResolution = new Vector2Int(1920, 1080);
        [DisplayName("屏幕匹配模式:")]
        public CanvasScaler.ScreenMatchMode MatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        [Range(0, 1)]
        [DisplayName("匹配宽度还是高度:")]
        public float MatchWidthOrHeight = 0;

        //[DisplayName("UI层级顺序:")]
        public List<UILayer> Layers = new List<UILayer>();

        public UIRuntimeConfig()
        {
            Type enumType = typeof(UILayer);
            string[] strs = Enum.GetNames(enumType);
            foreach (var item in strs)
            {
                UILayer uILayerEnum = (UILayer)Enum.Parse(enumType, item);
                Layers.Add(uILayerEnum);
            }
            Layers.Sort();
        }
    }
}
