using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using static Codice.CM.Common.Serialization.PacketFileReader;
using static MFramework.AssetMonitor.ProjectAssetMonitorTab;

namespace MFramework.AssetMonitor
{
    partial class ProjectAssetMonitorTab
    {
        /// 搜索文件树数据
        private List<TreeViewItemData<AssetInfoRecord>> _filteredItems = new List<TreeViewItemData<AssetInfoRecord>>();
        private List<SearchPart> _searchParts = new List<SearchPart>();
        /// <summary>
        /// 搜索文件树
        /// </summary>
        private void m_searchFolderTree()
        {
            if (string.IsNullOrEmpty(_currentSearchText))
            {
                // 没有搜索文本，显示所有项目
                _folderTreeView.SetRootItems(_allItems);
                return;
            }
            else
            {
                // 有搜索文本，过滤项目
                _filteredItems.Clear();
                // 解析高级搜索语法
                var searchParts = m_parseSearchText();
                foreach (var rootItem in _allItems)
                {
                    var filteredItem = m_searchFolderTreeItem(rootItem);
                    if (filteredItem.data != null)
                    {
                        _filteredItems.Add(filteredItem);
                    }
                }
                _folderTreeView.SetRootItems(_filteredItems);
            }
            _folderTreeView.Rebuild();
            // 如果有搜索结果，展开所有节点以便查看
            if (!string.IsNullOrEmpty(_currentSearchText) && _filteredItems.Count > 0)
            {
                m_selectTreeViewItem(_filteredItems[0], true);
            }
        }

        private TreeViewItemData<AssetInfoRecord> m_searchFolderTreeItem(TreeViewItemData<AssetInfoRecord> item)
        {
            AssetInfoRecord record = item.data;
            bool matchesCurrent = m_matchesSearchCriteria(record);

            List<TreeViewItemData<AssetInfoRecord>> filteredChildren = new List<TreeViewItemData<AssetInfoRecord>>();

            // 递归过滤子项
            if (item.hasChildren)
            {
                foreach (var child in item.children)
                {
                    var filteredChild = m_searchFolderTreeItem(child);
                    if (filteredChild.data != null)
                    {
                        filteredChildren.Add(filteredChild);
                    }
                }
            }

            // 如果当前项匹配或有匹配的子项，则包含此项
            if (matchesCurrent || filteredChildren.Count > 0)
            {
                return new TreeViewItemData<AssetInfoRecord>(item.id, record, filteredChildren);
            }

            return default;
        }

        // 添加搜索文本解析方法
        private List<SearchPart> m_parseSearchText()
        {
            string searchText = _currentSearchText;
            _searchParts.Clear();
            var tokens = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in tokens)
            {
                string prefix = "";
                string value = "";

                if (token.Contains(':'))
                {
                    var colonIndex = token.IndexOf(':');
                    prefix = token.Substring(0, colonIndex).ToLower();
                    value = token.Substring(colonIndex + 1);
                }
                else
                {
                    value = token;
                }
                SearcherInfo searchinfo = AssetMonitorTools.GetSearcherInfoByOption(prefix);

                if (searchinfo == null)
                    continue;
                _searchParts.Add(new SearchPart
                {
                    Info = searchinfo,
                    Value = value
                });
            }

            return _searchParts;
        }

        // 添加搜索条件匹配方法
        private bool m_matchesSearchCriteria(AssetInfoRecord record)
        {
            if (_searchParts.Count == 0)
                return true;
            foreach (var part in _searchParts)
            {
                var searcher = part.Info.Searcher;
                if (searcher == null)
                    continue;
                if (!searcher.Match(record, part.Value))
                    return false;
            }

            return true;
        }


        public class SearchPart
        {
            public SearcherInfo Info;
            public string Value;
        }
    }
}
