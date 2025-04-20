using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    public sealed class TestRuntimeConfig : ICustomMicroRuntimeConfig
    {
        [DisplayName("设计分辨率:")]
        public Vector2Int DesignResolution = new Vector2Int(1920, 1080);
        [DisplayName("屏幕匹配模式:")]
        public CanvasScaler.ScreenMatchMode MatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        [Range(0, 1)]
        [DisplayName("匹配宽度还是高度:")]
        public float MatchWidthOrHeight = 0;

    }
    internal class TestWindow:EditorWindow
    {
        [MenuItem("MFramework/Test Window")]
        static void Init()
        {
            var window = GetWindow<TestWindow>();
            window.Show();
        }
        private void CreateGUI()
        {
            VisualElement root = this.rootVisualElement;
            //root.Add(new MicroObjectField(new TestRuntimeConfig()));
        }
    }
}
