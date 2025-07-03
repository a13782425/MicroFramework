using UnityEngine.UIElements;

namespace MFramework.AssetMonitor
{
    /// <summary>
    /// 资源监控的标签
    /// </summary>
    public abstract class BaseAssetMonitorTab
    {

        /// <summary>
        /// 容器
        /// </summary>
        private VisualElement _container;
        internal protected VisualElement container => _container;
        protected AssetMonitorWindow window { get; private set; }
        /// <summary>
        /// 标题
        /// </summary>
        internal protected abstract string title { get; }
        /// <summary>
        /// Unity内置图标
        /// </summary>
        internal protected abstract string icon { get; }
        /// <summary>
        /// 鼠标放上去的提示
        /// </summary>
        internal protected virtual string tooltip { get; }
        /// <summary>
        /// 优先级
        /// </summary>
        internal protected virtual int priority { get; } = 0;

        public BaseAssetMonitorTab()
        {
            _container = new VisualElement();
            _container.AddToClassList(AssetMonitorConst.USS_PAGE_CONTAINER_CLASS);
        }
        public virtual void Init(AssetMonitorWindow window)
        {
            this.window = window;
        }

        public virtual void Show()
        {

        }

        public virtual void Hide()
        {

        }
        public virtual void Exit()
        {

        }
        public void Add(VisualElement element) => _container.Add(element);

        internal virtual void OnUpdate() { }
    }
}
