using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace MFramework.Core.Editor
{
    internal static class VisualElementListPool
    {
        private static EditorObjectPool<List<VisualElement>> pool = new EditorObjectPool<List<VisualElement>>(20);

        public static List<VisualElement> Copy(List<VisualElement> elements)
        {
            List<VisualElement> list = pool.Get();
            list.AddRange(elements);
            return list;
        }

        public static List<VisualElement> Get(int initialCapacity = 0)
        {
            List<VisualElement> list = pool.Get();
            if (initialCapacity > 0 && list.Capacity < initialCapacity)
            {
                list.Capacity = initialCapacity;
            }
            return list;
        }

        public static void Release(List<VisualElement> elements)
        {
            elements.Clear();
            pool.Release(elements);
        }
    }
}
