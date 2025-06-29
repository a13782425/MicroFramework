//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine.UIElements;

//namespace MFramework.Core.Editor
//{
//    [CustomDrawer(typeof(List<>))]
//    internal class ListCustomDrawer : ICustomDrawer
//    {

//        public CustomDrawerType DrawerType => CustomDrawerType.Basics;
        
//        public VisualElement DrawUI(object target, FieldInfo fieldInfo, VisualElement originalElement = null)
//        {
//            var list = (IList)fieldInfo.GetValue(target);
//            if (list == null)
//            {
//                list = (IList)Activator.CreateInstance(fieldInfo.FieldType);
//                fieldInfo.SetValue(target, list);
//            }
//            Type genericType = fieldInfo.FieldType.GetGenericArguments()[0];
//            ListView listView = new ListView();
//            listView.showFoldoutHeader = true;
//            listView.headerTitle = fieldInfo.GetDisplayName();
//            listView.itemsSource = list;
//            listView.fixedItemHeight = 100;
//            listView.reorderable = true;
//            listView.reorderMode = ListViewReorderMode.Animated;
//            listView.makeItem = () => new VisualElement();
//            listView.bindItem = (element, i) =>
//            {
//                element.Clear();    
//                var item = list[i];
//                list[i] = item;
//                element.Add(item.DrawUI());
//            };
//            return listView;
//        }
//    }
//}