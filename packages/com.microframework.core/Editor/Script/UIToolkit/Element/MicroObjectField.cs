using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    public sealed class MicroObjectField : VisualElement
    {
        #region 常量定义

        private const string STYLE_PATH = "UIToolkit/Element/MicroObjectFieldStyle";
        private const string BASE_CLASS = "micro-object-field";
        private const string HEADER_CLASS = BASE_CLASS + "__header";
        private const string FOLDOUT_CLASS = BASE_CLASS + "__foldout";
        private const string CONTAINER_CLASS = BASE_CLASS + "__container";
        private const string NULL_LABEL_CLASS = BASE_CLASS + "__null";

        private const int NESTING_WIDTH = 15;
        #endregion

        #region 私有字段

        private static Type[] _unityTypes = new Type[]
        {
            typeof(Vector2),
            typeof(Vector2Int),
            typeof(Vector3),
            typeof(Vector3Int),
            typeof(Vector4),
            typeof(Quaternion),
            typeof(Color),
            typeof(Rect),
            typeof(Bounds),
            typeof(LayerMask),
        };

        private readonly Func<object> _valueGetter;
        private readonly Action<object> _valueSetter;
        //private bool IsReadOnly => _valueSetter == null || _category == ObjectCategory.Struct;
        private readonly bool _needShowChildren = false;
        private object _value;
        private MicroObjectField _parent = null;
        private Foldout _foldout;
        private VisualElement _headerContainer;
        private VisualElement _valueContainer;
        //private ObjectCategory _category;
        private int _nestingLevel;

        private bool _showFoldout;

        private List<FieldInfo> _fieldInfos = new List<FieldInfo>();
        #endregion

        #region 属性定义
        /// <summary>
        /// 父级对象
        /// </summary>
        public MicroObjectField Parent => _parent;
        /// <summary>
        /// 缩进层级
        /// </summary>
        public int NestingLevel => _showFoldout ? _nestingLevel + 1 : _nestingLevel;

        /// <summary>
        /// 是否显示折叠按钮
        /// </summary>
        public bool ShowFoldout
        {
            get => _showFoldout;
            set
            {
                if (_showFoldout == value)
                    return;
                _showFoldout = value;
                if (value)
                {
                    _foldout.style.display = DisplayStyle.Flex;
                }
                else
                    _foldout.style.display = DisplayStyle.None;
                _valueContainer.style.marginLeft = NESTING_WIDTH * NestingLevel;
            }
        }

        /// <summary>
        /// 对象值
        /// </summary>
        public object Value
        {
            get => _value;
            set
            {
                if (value != null)
                {
                    if (!value.GetType().IsClass || value.GetType().IsAbstract)
                        throw new NotSupportedException("MicroObjectField only supports class types.");
                }
                if (_value != value)
                {
                    _value = value;
                    Rebuild();
                }
            }
        }
        #endregion

        #region 构造函数

        /// <summary>
        /// 创建只读对象查看器
        /// </summary>
        public MicroObjectField(object target)
        {
            if (target != null)
            {
                if (!target.GetType().IsClass || target.GetType().IsAbstract)
                    throw new NotSupportedException("MicroObjectField only supports class types.");
            }
            _value = target;
            //_headerContainer = new VisualElement { name = "Header" };
            //_headerContainer.AddToClassList(HEADER_CLASS);
            //_valueContainer = new VisualElement { name = "ValueContainer" };
            //_valueContainer.AddToClassList(CONTAINER_CLASS);
            //this.Add(_headerContainer);
            //this.Add(_valueContainer);
            m_buildHeader();
            m_buildValueContainer();
            Rebuild();
        }


        #endregion

        public void Rebuild()
        {
            m_analyzeFileInfos();
            if (_showFoldout)
            {
                _foldout.style.display = DisplayStyle.Flex;
            }
            else
            {
                _foldout.style.display = DisplayStyle.None;
                _valueContainer.style.display = DisplayStyle.Flex;
            }
            _valueContainer.style.marginLeft = NESTING_WIDTH * NestingLevel;
            _valueContainer.Clear();
            m_showFieldElement();
        }


        #region UI构建方法
        private void AddNullDisplay()
        {
            var nullLabel = new Label("null") { name = "NullLabel" };
            nullLabel.AddToClassList(NULL_LABEL_CLASS);
            Add(nullLabel);
        }
        private void m_buildHeader()
        {
            _headerContainer = new VisualElement { name = "Header" };
            _headerContainer.AddToClassList(HEADER_CLASS);
            this.Add(_headerContainer);
            _foldout = new Foldout { value = true };
            _foldout.text = MicroContextEditorUtils.GetDisplayName(Value);
            _foldout.AddToClassList(FOLDOUT_CLASS);
            _foldout.RegisterValueChangedCallback(m_onFoldoutToggled);
            _headerContainer.Add(_foldout);
        }
        private void m_buildValueContainer()
        {
            _valueContainer = new VisualElement { name = "ValueContainer" };
            _valueContainer.AddToClassList(CONTAINER_CLASS);
            this.Add(_valueContainer);
        }
        private void m_onFoldoutToggled(ChangeEvent<bool> evt)
        {
            _valueContainer.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void m_showFieldElement()
        {
            foreach (var item in _fieldInfos)
            {
                List<ICustomDrawer> drawerList = MicroContextEditorUtils.GetDrawers(item);
                drawerList.Sort(Comparer<ICustomDrawer>.Default);
                foreach (var drawer in drawerList)
                {
                    try
                    {
                        var element = drawer.DrawUI(this, item);
                        if (element != null)
                            _valueContainer.Add(element);
                    }
                    catch (Exception ex)
                    {
                        MicroContext.logger.LogError($"对象{Value.GetType().Name},字段{item.Name},CustomDrawer执行失败:{ex.Message}");
                    }
                }
            }
        }
        #endregion

        #region 其他方法

        private void m_analyzeFileInfos()
        {
            _fieldInfos.Clear();
            if (Value == null)
                return;
            var fields = Value.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<NonSerializedAttribute>() != null)
                    continue;
                if (field.GetCustomAttribute<IgnoreAttribute>() != null)
                    continue;
                bool canSerialized = field.IsPublic || field.GetCustomAttribute<SerializeField>() != null || field.GetCustomAttribute<SerializeReference>() != null;
                if (!canSerialized)
                    continue;
                _fieldInfos.Add(field);
            }
        }

        private bool m_buildInType(Type fieldType, bool isGeneric = false)
        {
            if (fieldType.IsArray)
            {
                if (isGeneric)
                    return false;
                else
                    return m_buildInType(fieldType.GetElementType(), true);
            }
            if (fieldType.IsGenericType)
            {
                if (isGeneric)
                    return false;
                if (fieldType.GetGenericTypeDefinition() == typeof(List<>))
                    return m_buildInType(fieldType.GetGenericArguments()[0], true);
                else
                    return false;
            }
            if (fieldType.IsClass && !fieldType.IsAbstract)
                return true;
            if (fieldType.IsEnum)
                return true;
            if (fieldType.IsPrimitive)
                return true;
            return _unityTypes.Contains(fieldType);
        }

        #endregion
    }
}
//    /// <summary>
//    /// 通用对象查看器 - 支持嵌套结构/集合类型/自定义绘制器
//    /// </summary>
//    internal class MicroObjectField : VisualElement
//    {
//        #region 类型定义

//        private enum ObjectCategory
//        {
//            Null,
//            Primitive,
//            Enum,
//            Array,
//            List,
//            Dictionary,
//            Struct,
//            ClassObject,
//            UnityObject
//        }

//        #endregion

//        #region 常量定义

//        private const string STYLE_PATH = "UIToolkit/Element/MicroObjectFieldStyle";
//        private const string BASE_CLASS = "micro-object-field";
//        private const string HEADER_CLASS = BASE_CLASS + "__header";
//        private const string FOLDOUT_CLASS = BASE_CLASS + "__foldout";
//        private const string CONTAINER_CLASS = BASE_CLASS + "__container";
//        private const string NULL_LABEL_CLASS = BASE_CLASS + "__null";

//        private const int NESTING_WIDTH = 15;
//        #endregion

//        #region 私有字段

//        private readonly object _target;
//        private readonly Func<object> _valueGetter;
//        private readonly Action<object> _valueSetter;
//        private bool IsReadOnly => _valueSetter == null || _category == ObjectCategory.Struct;
//        private readonly bool _needShowChildren = false;

//        private MicroObjectField _parent = null;
//        private Foldout _foldout;
//        private VisualElement _valueContainer;
//        private ObjectCategory _category;
//        private int _nestingLevel;
//        #endregion

//        #region 属性定义
//        /// <summary>
//        /// 父级对象
//        /// </summary>
//        public MicroObjectField Parent => _parent;
//        public FieldInfo FieldInfo;
//        /// <summary>
//        /// 缩进层级
//        /// </summary>
//        public int NestingLevel => _nestingLevel;
//        #endregion

//        #region 构造函数

//        /// <summary>
//        /// 创建只读对象查看器
//        /// </summary>
//        public MicroObjectField(object target) : this(target, null, null)
//        {
//            _needShowChildren = true;
//        }

//        /// <summary>
//        /// 创建可编辑对象查看器
//        /// </summary>
//        private MicroObjectField(
//            object target,
//            Func<object> getter = null,
//            Action<object> setter = null)
//        {
//            // 构建UI
//            // 初始化样式和布局
//            this.AddStyleSheet(STYLE_PATH);
//            AddToClassList(BASE_CLASS);
//            // 初始化数据绑定
//            _target = target;
//            if (_target == null)
//            {
//                AddNullDisplay();
//                return;
//            }


//            // 分析对象类型
//            _category = AnalyzeObjectType();

//            _valueGetter = getter;
//            _valueSetter = setter;

//            // 构建UI
//            BuildHeader();
//            if (ShouldShowChildrenImmediately)
//            {
//                BuildValueContainer();
//            }
//        }

//        #endregion

//        #region UI构建方法
//        private void AddNullDisplay()
//        {
//            var nullLabel = new Label("null") { name = "NullLabel" };
//            nullLabel.AddToClassList(NULL_LABEL_CLASS);
//            Add(nullLabel);
//        }
//        private void BuildHeader()
//        {
//            var header = new VisualElement { name = "Header" };
//            header.AddToClassList(HEADER_CLASS);

//            // 折叠按钮
//            if (CanHaveChildren)
//            {
//                _foldout = new Foldout { value = true };
//                _foldout.AddToClassList(FOLDOUT_CLASS);
//                _foldout.RegisterValueChangedCallback(OnFoldoutToggled);
//                _foldout.Q<Toggle>().text = GetTypeName();
//                header.Add(_foldout);
//            }

//            Add(header);
//        }

//        private void BuildValueContainer()
//        {
//            _valueContainer = new VisualElement { name = "ValueContainer" };
//            _valueContainer.AddToClassList(CONTAINER_CLASS);
//            _valueContainer.style.marginLeft = NESTING_WIDTH * _nestingLevel;

//            switch (_category)
//            {
//                case ObjectCategory.Array:
//                    RenderArrayElements();
//                    break;
//                case ObjectCategory.List:
//                    RenderListElements();
//                    break;
//                case ObjectCategory.Dictionary:
//                    RenderDictionaryElements();
//                    break;
//                case ObjectCategory.Struct:
//                case ObjectCategory.ClassObject:
//                    RenderObjectFields();
//                    break;
//                case ObjectCategory.UnityObject:
//                    RenderUnityObjectField();
//                    break;
//            }

//            Add(_valueContainer);
//        }

//        #endregion

//        #region 类型处理方法

//        private ObjectCategory AnalyzeObjectType()
//        {
//            if (_target == null) return ObjectCategory.Null;

//            Type type = _target.GetType();

//            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
//                return ObjectCategory.UnityObject;

//            if (type.IsEnum)
//                return ObjectCategory.Enum;

//            if (type.IsArray)
//                return ObjectCategory.Array;

//            if (type.IsGenericType)
//            {
//                Type genericDef = type.GetGenericTypeDefinition();
//                if (genericDef == typeof(List<>))
//                    return ObjectCategory.List;
//                if (genericDef == typeof(Dictionary<,>))
//                    return ObjectCategory.Dictionary;
//            }

//            if (type.IsValueType)
//            {
//                return type.IsPrimitive ?
//                    ObjectCategory.Primitive :
//                    ObjectCategory.Struct;
//            }

//            return ObjectCategory.ClassObject;
//        }

//        private string GetTypeName()
//        {
//            if (_target == null) return "Null";

//            Type type = _target.GetType();
//            if (type.IsArray)
//                return $"{type.GetElementType().Name}[]";

//            if (type.IsGenericType)
//            {
//                var args = type.GetGenericArguments();
//                return type.Name.Split('`')[0] +
//                       $"<{string.Join(",", args.Select(t => t.Name))}>";
//            }

//            return type.Name;
//        }

//        #endregion

//        #region 数据渲染方法

//        private void RenderArrayElements()
//        {
//            var array = _target as Array;
//            for (int i = 0; i < array.Length; i++)
//            {
//                int index = i; // 闭包捕获当前索引
//                AddElementField(
//                    label: $"[{index}]",
//                    value: array.GetValue(index),
//                    getter: () => array.GetValue(index),
//                    setter: v => array.SetValue(v, index)
//                );
//            }
//        }

//        private void RenderListElements()
//        {
//            var list = _target as IList;
//            for (int i = 0; i < list.Count; i++)
//            {
//                int index = i;
//                AddElementField(
//                    label: $"[{index}]",
//                    value: list[index],
//                    getter: () => list[index],
//                    setter: v => list[index] = v
//                );
//            }
//        }

//        private void RenderDictionaryElements()
//        {
//            var dict = _target as IDictionary;
//            foreach (DictionaryEntry entry in dict)
//            {
//                var key = entry.Key;
//                var value = entry.Value;

//                var pairContainer = new VisualElement
//                {
//                    style = { flexDirection = FlexDirection.Row }
//                };

//                // Key字段（只读）
//                pairContainer.Add(new MicroObjectField(key)
//                {
//                    style = { flexGrow = 1, marginRight = 5 }
//                });

//                // Value字段
//                pairContainer.Add(new MicroObjectField(value,
//                    getter: () => dict[key],
//                    setter: v => dict[key] = v)
//                {
//                    style = { flexGrow = 1 }
//                });

//                _valueContainer.Add(pairContainer);
//            }
//        }

//        private void RenderObjectFields()
//        {
//            var fields = GetFieldInfos();

//            foreach (var field in fields)
//            {
//                AddElementField(
//                    label: field.Name,
//                    value: field.GetValue(_target),
//                    getter: () => field.GetValue(_target),
//                    setter: v => field.SetValue(_target, v)
//                );
//            }
//        }

//        private void RenderUnityObjectField()
//        {
//            var unityObject = _target as UnityEngine.Object;
//            var objectField = new ObjectField
//            {
//                value = unityObject,
//                objectType = unityObject.GetType()
//            };

//            if (!IsReadOnly)
//            {
//                objectField.RegisterValueChangedCallback(evt =>
//                {
//                    _valueSetter?.Invoke(evt.newValue);
//                });
//            }

//            _valueContainer.Add(objectField);
//        }

//        private void AddElementField(
//            string label,
//            object value,
//            Func<object> getter,
//            Action<object> setter)
//        {
//            var fieldContainer = new VisualElement
//            {
//                style =
//                {
//                    flexDirection = FlexDirection.Row,
//                    marginTop = 2
//                }
//            };

//            // 字段标签
//            fieldContainer.Add(new Label(label)
//            {
//                style =
//                {
//                    width = 120,
//                    unityTextAlign = TextAnchor.MiddleLeft
//                }
//            });

//            // 值字段
//            var valueField = new MicroObjectField(value, getter, setter)
//            {
//                style = { flexGrow = 1 }
//            };

//            fieldContainer.Add(valueField);
//            _valueContainer.Add(fieldContainer);
//        }

//        private List<FieldInfo> GetFieldInfos()
//        {
//            if (_target == null)
//                return new List<FieldInfo>();
//            return _target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(a =>
//                        {
//                            bool hasSerialized = a.GetCustomAttribute<SerializeField>() != null;
//                            bool hasNoSerialized = a.GetCustomAttribute<NonSerializedAttribute>() != null;
//                            if (hasNoSerialized)
//                                return false;
//                            if (hasSerialized)
//                                return true;
//                            return a.IsPublic;
//                        }).ToList();
//        }

//        #endregion

//        #region 辅助属性

//        private bool CanHaveChildren => _category switch
//        {
//            ObjectCategory.Array
//                or ObjectCategory.List
//                or ObjectCategory.Dictionary
//                or ObjectCategory.Struct
//                or ObjectCategory.ClassObject => true,
//            _ => false
//        };

//        /// <summary>
//        /// 是否需要显示子元素
//        /// </summary>
//        private bool ShouldShowChildrenImmediately => _category switch
//        {
//            ObjectCategory.Struct or ObjectCategory.ClassObject => true,
//            _ => false
//        };

//        private string GetValuePreview()
//        {
//            if (_target == null) return "null";

//            return _category switch
//            {
//                ObjectCategory.Array => $"Array [{((Array)_target).Length}]",
//                ObjectCategory.List => $"List [{((IList)_target).Count}]",
//                ObjectCategory.Dictionary => $"Dictionary [{((IDictionary)_target).Count}]",
//                ObjectCategory.UnityObject => ((UnityEngine.Object)_target).name,
//                _ => _target.ToString()
//            };
//        }

//        private string GetValueTooltip() => _target?.GetType().AssemblyQualifiedName;

//        #endregion

//        #region 事件处理

//        private void OnFoldoutToggled(ChangeEvent<bool> evt)
//        {
//            if (evt.newValue)
//            {
//                if (_valueContainer == null)
//                {
//                    BuildValueContainer();
//                }
//                else
//                {
//                    _valueContainer.style.display = DisplayStyle.Flex;
//                }
//                //_foldout.Q<Toggle>().RemoveFromClassList("unity-foldout__toggle");
//            }
//            else
//            {
//                _valueContainer.style.display = DisplayStyle.None;
//            }
//        }

//        #endregion


//        #region Class

//        private class DrawerProcessor
//        {
//            public object Target { get; }
//            public FieldInfo Field { get; }
//            public Func<object> Getter { get; }
//            public Action<object> Setter { get; }

//            private VisualElement _root;
//            private VisualElement _originalElement;

//            public DrawerProcessor(
//                object target,
//                FieldInfo field,
//                Func<object> getter,
//                Action<object> setter)
//            {
//                Target = target;
//                Field = field;
//                Getter = getter;
//                Setter = setter;
//            }

//            public VisualElement Process()
//            {
//                var drawers = MicroContextEditor
//                    .GetCustomDrawers(Field)
//                    .GroupBy(d => d.DrawerType)
//                    .ToDictionary(g => g.Key, g => g.ToList());

//                // 阶段1: PreDecorate
//                _root = new VisualElement();
//                ApplyDrawers(drawers, CustomDrawerType.PreDecorate);

//                // 阶段2: Basics
//                var basics = GetFirstBasicsDrawer(drawers);
//                if (basics != null)
//                {
//                    _originalElement = basics.DrawUI(Target, Field, Getter, Setter, null);
//                    if (_originalElement != null)
//                    {
//                        _root.Add(_originalElement);
//                    }
//                }
//                else
//                {
//                    // 默认绘制
//                    _originalElement = CreateDefaultElement();
//                    _root.Add(_originalElement);
//                }

//                // 阶段3: Modify
//                if (_originalElement != null)
//                {
//                    ApplyModifyDrawers(drawers);
//                }

//                // 阶段4: NextDecorate
//                ApplyDrawers(drawers, CustomDrawerType.NextDecorate);

//                return _root;
//            }

//            private void ApplyDrawers(
//                Dictionary<CustomDrawerType, List<ICustomDrawer>> drawers,
//                CustomDrawerType type)
//            {
//                if (!drawers.ContainsKey(type)) return;

//                foreach (var drawer in drawers[type])
//                {
//                    var element = drawer.DrawUI(Target, Field, Getter, Setter);
//                    if (element != null)
//                    {
//                        _root.Add(element);
//                    }
//                }
//            }

//            private void ApplyModifyDrawers(Dictionary<CustomDrawerType, List<ICustomDrawer>> drawers)
//            {
//                if (!drawers.ContainsKey(CustomDrawerType.Modify)) return;

//                foreach (var drawer in drawers[CustomDrawerType.Modify])
//                {
//                    var modified = drawer.DrawUI(Target, Field, Getter, Setter, _originalElement);
//                    if (modified != null && modified != _originalElement)
//                    {
//                        // 替换元素
//                        var index = _root.IndexOf(_originalElement);
//                        _root.Remove(_originalElement);
//                        _root.Insert(index, modified);
//                        _originalElement = modified;
//                    }
//                }
//            }

//            private ICustomDrawer GetFirstBasicsDrawer(Dictionary<CustomDrawerType, List<ICustomDrawer>> drawers)
//            {
//                return drawers.ContainsKey(CustomDrawerType.Basics) ?
//                    drawers[CustomDrawerType.Basics].FirstOrDefault() :
//                    null;
//            }

//            private VisualElement CreateDefaultElement()
//            {
//                // 原MicroObjectField的默认创建逻辑
//                return new MicroObjectField(Getter(), Getter, Setter);
//            }
//        }

//        #endregion
//    }
//}