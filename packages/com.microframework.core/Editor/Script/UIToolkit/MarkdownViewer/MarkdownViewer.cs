using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using MFramework;
using Object = UnityEngine.Object;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    public class MarkdownViewer : VisualElement
    {
        private string m_Path;

        private UIMarkdownRenderer m_Renderer;
        public MarkdownViewer()
        {
            m_Renderer = new UIMarkdownRenderer("", HandleLink, rootPath: Application.dataPath);
            Add(m_Renderer.RootElement);
        }

        public void SetMarkdown(string text)
        {
            m_Renderer.SetMarkdown(text);
        }

        public static void HandleLink(string link)
        {
            if (link.StartsWith("#"))
            {
                UIMarkdownRenderer.ScrollToHeader(link);
            }
            else if (link.StartsWith("Assets") || link.StartsWith("Packages"))
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(link);
                Selection.activeObject = obj;
            }
            else if (link.StartsWith("search:"))
            {
                //this is a relative link, so find the actual link
                link = link.Replace("search:", "");

                var files = AssetDatabase.FindAssets(link);

                if (files.Length == 0)
                {
                    Debug.LogError($"Couldn't find file {link}");
                    return;
                }

                link = AssetDatabase.GUIDToAssetPath(files[0]);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(link);
                Selection.activeObject = obj;
            }
            else if (link.StartsWith("package:"))
            {
                //will search only in packages
                link = link.Replace("package:", "");

                var files = AssetDatabase.FindAssets($"a:packages {link}");

                if (files.Length == 0)
                {
                    Debug.LogError($"Couldn't find link : {link}");
                    return;
                }

                link = AssetDatabase.GUIDToAssetPath(files[0]);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(link);
                Selection.activeObject = obj;
            }
            else if (link.StartsWith("cmd:"))
            {
                string cmdName;
                string[] parameters;

                link = link.Replace("cmd:", "");
                int openingParenthesis = link.IndexOf('(');
                if (openingParenthesis == -1)
                {
                    //no parameters
                    cmdName = link;
                    parameters = Array.Empty<string>();
                }
                else
                {
                    //we find the closing one
                    int closingParenthesis = link.IndexOf(')');

                    cmdName = link.Substring(0, openingParenthesis);
                    string parametersString = link.Substring(openingParenthesis + 1, closingParenthesis - openingParenthesis - 1);

                    parameters = parametersString.Split(',');

                }

                UIMarkdownRenderer.Command cmd = new UIMarkdownRenderer.Command()
                {
                    CommandName = cmdName,
                    CommandParameters = parameters
                };

                UIMarkdownRenderer.SendCommand(cmd);
            }
            else if (link.EndsWith(".md") || link.EndsWith(".txt"))
            {
                //this is a link to an external MD or text file so we open it with the viewer instead of using Application.OpenURL
                //if (!Path.IsPathRooted(link))
                //    link = Path.Combine(Path.GetDirectoryName(filePath), link);

                //Open(link);

            }
            else
            {
                //any other link is open normally
                Application.OpenURL(link);
            }

        }
    }
}