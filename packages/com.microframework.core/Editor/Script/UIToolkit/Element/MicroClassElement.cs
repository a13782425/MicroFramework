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
    /// 类序列化分组，一个组内的类型不能重复
    /// </summary>
    public class MicroClassGroup
    {
        private List<MicroClassElement> _allElementList = new List<MicroClassElement>();

        /// <summary>
        /// 检查类型是否存在于组内
        /// </summary>
        /// <param name="serializer"></param>
        /// <returns></returns>
        internal bool CheckTypeExist(string assemblyName, string typeName)
        {
            return _allElementList.Any(x => x.value == null ? false : x.value.AssemblyName == assemblyName && x.value.TypeName == typeName);
        }

        internal void AddMicroClassElemenet(MicroClassElement microClassElement)
        {
            if (!_allElementList.Contains(microClassElement))
                _allElementList.Add(microClassElement);
        }

        internal void RemoveMicroClassElemenet(MicroClassElement microClassElement)
        {
            _allElementList.Remove(microClassElement);
        }
    }

    /// <summary>
    /// 类序列化组件
    /// </summary>
    public class MicroClassElement : BaseField<MicroClassSerializer>
    {
        /// <summary>
        /// 默认程序集名字
        /// </summary>
        const string DEFAULT_ASSEMBLY = "Assembly-CSharp";

        const string k_UssPath = "UIToolkit/Uss/MicroClassElement";
        const string k_BassUssClassName = "micro-class-element";
        const string k_AssemblyDpFieldUssClassName = k_BassUssClassName + "__assembly-dp-field";
        const string k_ClassDpFieldUssClassName = k_BassUssClassName + "__class-dp-field";
        private MicroClassSerializer _serializer;

        private MicroDropdownField _assemblyDpField;
        private MicroDropdownField _classDpField;
        /// <summary>
        /// 筛选类型，如果为null则不筛选，筛选类型为null时会获取所有程序集的类型
        /// </summary>
        private Type _filterType;

        private List<Type> _allTypes = new List<Type>();

        private MicroClassGroup _group;
        /// <summary>
        /// 根据你设置的filterTypez会后可以在自定筛选一次,返回true采用,返回false不采用
        /// </summary>
        public event Func<Type, bool> onCustomFilterType;

        private bool _showAssemblyDpField = false;
        public bool showAssembly
        {
            get => _showAssemblyDpField;
            set
            {
                _showAssemblyDpField = value;
                _assemblyDpField.SetDisplay(value);
            }
        }

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
        public MicroClassElement(string label, MicroClassSerializer serializer) : this(label, serializer, group: null, null) { }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="filterType"></param>
        public MicroClassElement(string label, MicroClassSerializer serializer, Type filterType) : this(label, serializer, null, filterType) { }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serializer"></param>        
        public MicroClassElement(string label, MicroClassSerializer serializer, MicroClassGroup group) : this(label, serializer, group, null) { }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serializer"></param>        
        public MicroClassElement(string label, MicroClassSerializer serializer, Type filterType, MicroClassGroup group) : this(label, serializer, group, filterType) { }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="filterType"></param>
        public MicroClassElement(string label, MicroClassSerializer serializer, MicroClassGroup group, Type filterType) : this(label, visualInput: null)
        {
            _filterType = filterType;
            _group = group;
            this.RegisterCallback<AttachToPanelEvent>(onAttachToPanel);
            this.RegisterCallback<DetachFromPanelEvent>(onDetachToPanel);
            _assemblyDpField = new MicroDropdownField();
            _classDpField = new MicroDropdownField();
            _assemblyDpField.AddToClassList(k_AssemblyDpFieldUssClassName);
            _classDpField.AddToClassList(k_ClassDpFieldUssClassName);
            _assemblyDpField.value = string.IsNullOrWhiteSpace(_serializer?.AssemblyName) ? DEFAULT_ASSEMBLY : _serializer?.AssemblyName;
            _assemblyDpField.SetDisplay(false);
            this.Add(_assemblyDpField);
            this.Add(_classDpField);
            _assemblyDpField.getContent += _assemblyDpField_getContent;
            _assemblyDpField.onSelectionItemChanged += _assemblyDpField_onSelectItemChanged;
            _classDpField.getContent += _dropDownField_getContent;
            _classDpField.onSelectionItemChanged += onSelectItemChanged;
            this.value = serializer;
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public void Refresh()
        {
            if (_serializer == null)
                return;
            _classDpField.SetValueWithoutNotify(_serializer.TypeName);
            _assemblyDpField.SetValueWithoutNotify(string.IsNullOrWhiteSpace(_serializer.AssemblyName) ? DEFAULT_ASSEMBLY : _serializer.AssemblyName);
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
                return new MicroDropdownContent();
            _allTypes.Clear();
            string selectAssemblyName = string.IsNullOrWhiteSpace(_serializer.AssemblyName) ? DEFAULT_ASSEMBLY : _serializer.AssemblyName;
            if (_filterType == null)
            {
                foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (_showAssemblyDpField && item.FullName == selectAssemblyName)
                    {
                        _allTypes.AddRange(item.GetTypes());
                        break;
                    }
                    _allTypes.AddRange(item.GetTypes());
                }
            }
            else
            {
                _allTypes.AddRange(TypeCache.GetTypesDerivedFrom(_filterType));
                if (_showAssemblyDpField)
                    _allTypes.RemoveAll(a => a.Assembly.FullName != selectAssemblyName);
            }
            MicroDropdownContent microDropdownContent = new MicroDropdownContent();
            foreach (var item in _allTypes)
            {
                if (_group == null ? false : _group.CheckTypeExist(item.Assembly.FullName, item.FullName))
                    continue; //如果组内已经存在该类型则不添加
                bool res = true;
                if (onCustomFilterType != null)
                    res = onCustomFilterType.Invoke(item);
                if (res)
                    microDropdownContent.AppendValue(new MicroDropdownContent.ValueItem() { value = item.Assembly.FullName, displayName = item.FullName });
            }
            return microDropdownContent;
        }

        private MicroDropdownContent _assemblyDpField_getContent()
        {
            if (_serializer == null)
            {
                return new MicroDropdownContent();
            }
            MicroDropdownContent microDropdownContent = new MicroDropdownContent();
            foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
            {
                microDropdownContent.AppendValue(new MicroDropdownContent.ValueItem() { value = item.FullName, displayName = item.GetName().Name });
            }
            return microDropdownContent;
        }
        private void _assemblyDpField_onSelectItemChanged(MicroDropdownContent.ValueItem item)
        {
            if (_serializer == null)
                return;
            _serializer.AssemblyName = item.value;
        }


        /// <summary>
        /// 添加到面板时触发
        /// </summary>
        /// <param name="evt"></param>
        private void onAttachToPanel(AttachToPanelEvent evt)
        {
            this._group?.AddMicroClassElemenet(this);
        }

        /// <summary>
        /// 移除面板时触发
        /// </summary>
        /// <param name="evt"></param>
        private void onDetachToPanel(DetachFromPanelEvent evt)
        {
            this._group?.RemoveMicroClassElemenet(this);
        }
    }
}
