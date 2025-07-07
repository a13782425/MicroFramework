using MFramework.Core;
using MFramework.Runtime;

namespace MFramework.YooAsset
{
    [DisplayName("YooAsset路径")]
    internal class YooassetPathProcessor : IResourcePathProcessor
    {
        public string GetAssetPath(string originPath)
        {
            return originPath.Replace("\\", "/");
        }

        public bool IsValid(string originPath)
        {
            return originPath.StartsWith("Assets") || originPath.StartsWith("Packages");
        }
    }
}
