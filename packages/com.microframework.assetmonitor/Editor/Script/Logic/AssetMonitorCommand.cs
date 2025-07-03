using System.IO;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace MFramework.AssetMonitor
{
    /// <summary>
    /// 指令类型
    /// </summary>
    public enum CommandType
    {
        /// <summary>
        /// 文件夹列表
        /// </summary>
        Folder = 0,
        /// <summary>
        /// 关系列表
        /// </summary>
        Relation = 1,
        /// <summary>
        /// 验证列表
        /// </summary>
        Verify = 2,
    }

    /// <summary>
    /// 自定义右键指令
    /// </summary>
    public interface IAssetMonitorCommand
    {
        /// <summary>
        /// 自定义右键指令名
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 自定义指令描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 自定义指令优先级
        /// </summary>
        int Priority { get => 0; }

        /// <summary>
        /// 过滤guid, 根据guid判断是否使用该右键指令
        /// </summary>
        /// <param name="guid">资源文件的guid</param>
        /// <param name="commandType">触发该指令的类型</param>
        /// <returns></returns>
        bool OnFilter(string guid, CommandType commandType);

        /// <summary>
        /// 执行该条指令
        /// </summary>
        /// <param name="guid">资源文件的guid</param>
        /// <param name="commandType">触发该指令的类型</param>
        /// <returns>执行是否成功</returns>
        bool OnExecute(string guid, CommandType commandType);

    }

    internal class SelectAssetCommand : IAssetMonitorCommand
    {
        public string Name => "选中";

        public string Description => "";

        public bool OnExecute(string guid, CommandType commandType)
        {
            UnityObject obj = AssetDatabase.LoadAssetAtPath<UnityObject>(AssetDatabase.GUIDToAssetPath(guid));
            if (obj != null)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
                return true;
            }
            return false;
        }

        public bool OnFilter(string guid, CommandType commandType) => true;
    }

    internal class OpenInFinderAssetCommand : IAssetMonitorCommand
    {
        public string Name => "打开文件夹";
        public string Description => "";

        public bool OnExecute(string guid, CommandType commandType)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EditorUtility.RevealInFinder(Path.GetDirectoryName(path));
            return false;
        }

        public bool OnFilter(string guid, CommandType commandType) => true;
    }

    internal class DeleteAssetCommand : IAssetMonitorCommand
    {
        public string Name => "删除";
        public string Description => "";

        public bool OnExecute(string guid, CommandType commandType)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (EditorUtility.DisplayDialog("提示", $"是否删除资源:{Path.GetFileName(path)} ", "删除", "关闭"))
                return AssetDatabase.DeleteAsset(path);
            return true;
        }

        public bool OnFilter(string guid, CommandType commandType) => commandType == CommandType.Folder;
    }
}
