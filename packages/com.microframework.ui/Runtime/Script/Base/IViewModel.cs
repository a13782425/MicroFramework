using MFramework.Runtime;

namespace MFramework.UI
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public interface IViewModel : IBindable
    {
        /// <summary>
        /// 界面创建完成前初始化数据
        /// </summary>
        void OnCreate() { }
        /// <summary>
        /// 界面显示前初始化数据
        /// </summary>
        void OnEnable() { }
        /// <summary>
        /// 界面隐藏后释放数据
        /// </summary>
        void OnDisable() { }
        /// <summary>
        /// 界面销毁后释放数据
        /// </summary>
        void OnDestroy() { }
    }
}
