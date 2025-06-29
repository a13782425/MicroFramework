using MFramework.Core;

namespace MFramework.Runtime
{
    /// <summary>
    /// UIComponent拓展
    /// </summary>
    public static class UIComponentExtensions
    {
        public static UIObserver<TComponent, TModel> Bind<TComponent, TModel>(this TComponent component, TModel model) where TComponent : UIComponent where TModel : class, IBindable
        {
            UIObserver<TComponent, TModel> observer = new UIObserver<TComponent, TModel>(component, model);
            return observer;
        }
    }
}
