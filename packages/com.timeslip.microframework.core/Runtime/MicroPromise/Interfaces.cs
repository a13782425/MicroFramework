namespace MFramework.Core
{
    /// <summary>
    /// Cancelable interface
    /// </summary>
    public interface ICancelable
    {
        /// <summary>
        /// Cancel this instance.
        /// </summary>
        void Cancel();
    }
}