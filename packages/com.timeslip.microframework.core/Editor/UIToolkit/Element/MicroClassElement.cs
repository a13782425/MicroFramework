using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    /// <summary>
    /// 类序列化组件
    /// </summary>
    public class MicroClassElement : BaseField<MicroClassSerializer>
    {
        const string k_UssPath = "UIToolkit/Uss/MicroClassElement";
        private MicroClassSerializer _serializer;
        private MicroDropdownField _dropDownField;
        private Type _filterType;
        private List<Type> _allTypes = new List<Type>();

        /// <summary>
        /// 根据你设置的filterTypez会后可以在自定筛选一次,返回true采用,返回false不采用
        /// </summary>
        public event Func<Type, bool> onCustomFilterType;

        public override MicroClassSerializer value
        {
            get => _serializer;
            set
            {
                _serializer = value;
                Refresh();
            }
        }

        protected MicroClassElement(string label, VisualElement visualInput) : base(label, visualInput)
        {
            this.AddStyleSheet(k_UssPath);
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="filterType"></param>
        public MicroClassElement(string label, MicroClassSerializer serializer, Type filterType) : this(label, null)
        {
            _serializer = serializer;
            _filterType = filterType;
            _dropDownField = new MicroDropdownField();
            this.Add(_dropDownField);
            _dropDownField.getContent += _dropDownField_getContent;
            _dropDownField.onSelectionItemChanged += onSelectItemChanged;
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public void Refresh()
        {
            if (_serializer == null)
                return;
            _dropDownField.SetValueWithoutNotify(_serializer.TypeName);
        }

        private void onSelectItemChanged(MicroDropdownContent.ValueItem item)
        {
            if (_serializer == null)
                return;
            _serializer.AssemblyName = item.value;
            _serializer.TypeName = item.displayName;
        }

        private MicroDropdownContent _dropDownField_getContent()
        {
            if (_serializer == null)
            {
                return new MicroDropdownContent();
            }
            _allTypes.Clear();
            if (_filterType == null)
            {
                foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
                {
                    _allTypes.AddRange(item.GetTypes());
                }
            }
            else
            {
                _allTypes.AddRange(TypeCache.GetTypesDerivedFrom(_filterType));
            }
            MicroDropdownContent microDropdownContent = new MicroDropdownContent();
            foreach (var item in _allTypes)
            {
                bool res = true;
                if (onCustomFilterType != null)
                {
                    res = onCustomFilterType.Invoke(item);
                }
                if (res)
                    microDropdownContent.AppendValue(new MicroDropdownContent.ValueItem() { value = item.Assembly.GetName().Name, displayName = item.FullName });
            }
            return microDropdownContent;
        }
    }
}
