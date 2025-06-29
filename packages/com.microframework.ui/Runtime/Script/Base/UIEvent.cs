using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MFramework.UI
{
    /// <summary>
    /// 所有UI事件
    /// </summary>
    public class UIEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IScrollHandler, IUpdateSelectedHandler, ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler, ICancelHandler, IEventSystemHandler
    {
        public event Action<PointerEventData> onBeginDrag;
        public event Action<PointerEventData> onDrag;
        public event Action<PointerEventData> onEndDrag;
        public event Action<PointerEventData> onPointerEnter;
        public event Action<PointerEventData> onPointerExit;
        public event Action<PointerEventData> onPointDown;
        public event Action<PointerEventData> onDrop;
        public event Action<PointerEventData> onPointerUp;
        public event Action<PointerEventData> onPointerClick;
        public event Action<PointerEventData> onInitializePotentialDrag;
        public event Action<PointerEventData> onScroll;
        public event Action<BaseEventData> onUpdateSelected;
        public event Action<BaseEventData> onSelect;
        public event Action<BaseEventData> onDeselect;
        public event Action<BaseEventData> onSubmit;
        public event Action<BaseEventData> onCancel;
        public event Action<AxisEventData> onMove;
        public static UIEvent Get(GameObject obj)
        {
            UIEvent @event = obj.GetComponent<UIEvent>();
            if (@event == null)
                return obj.AddComponent<UIEvent>();
            return @event;
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            onBeginDrag?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            onDrag?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            onEndDrag?.Invoke(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onPointerExit?.Invoke(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            onPointDown?.Invoke(eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            onDrop?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onPointerUp?.Invoke(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onPointerClick?.Invoke(eventData);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            onInitializePotentialDrag?.Invoke(eventData);
        }

        public void OnScroll(PointerEventData eventData)
        {
            onScroll?.Invoke(eventData);
        }

        public void OnUpdateSelected(BaseEventData eventData)
        {
            onUpdateSelected?.Invoke(eventData);
        }

        public void OnSelect(BaseEventData eventData)
        {
            onSelect?.Invoke(eventData);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            onDeselect?.Invoke(eventData);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            onSubmit?.Invoke(eventData);
        }

        public void OnCancel(BaseEventData eventData)
        {
            onCancel?.Invoke(eventData);
        }
        public void OnMove(AxisEventData eventData)
        {
            onMove?.Invoke(eventData);
        }
    }

    public abstract class UIEventBase<T> : MonoBehaviour where T : UIEventBase<T>
    {
        public static T Get(GameObject obj)
        {
            T @event = obj.GetComponent<T>();
            if (@event == null)
                return obj.AddComponent<T>();
            return @event;
        }
        protected Action<PointerEventData> pointerCallback;
        protected Action<BaseEventData> baseCallback;
        protected Action<AxisEventData> axisCallback;
    }
    //IPointerEnterHandler
    public class UIPointerEnterEvent : UIEventBase<UIPointerEnterEvent>, IPointerEnterHandler
    {
        public event Action<PointerEventData> onPointerEnter { add { pointerCallback += value; } remove { pointerCallback -= value; } }

        public void OnPointerEnter(PointerEventData eventData) => pointerCallback?.Invoke(eventData);
    }
    //IPointerExitHandler
    public class UIPointerExitEvent : UIEventBase<UIPointerExitEvent>, IPointerExitHandler
    {
        public event Action<PointerEventData> onPointerExit { add { pointerCallback += value; } remove { pointerCallback -= value; } }

        public void OnPointerExit(PointerEventData eventData) => pointerCallback?.Invoke(eventData);
    }
    //IPointerDownHandler
    public class UIPointerDownEvent : UIEventBase<UIPointerDownEvent>, IPointerDownHandler
    {
        public event Action<PointerEventData> onPointDown { add { pointerCallback += value; } remove { pointerCallback -= value; } }
        public void OnPointerDown(PointerEventData eventData) => pointerCallback?.Invoke(eventData);
    }
    //IPointerUpHandler
    public class UIPointerUpEvent : UIEventBase<UIPointerUpEvent>, IPointerUpHandler
    {
        public event Action<PointerEventData> onPointerUp { add { pointerCallback += value; } remove { pointerCallback -= value; } }

        public void OnPointerUp(PointerEventData eventData) => pointerCallback?.Invoke(eventData);
    }
    //IPointerClickHandler
    public class UIPointerClickEvent : UIEventBase<UIPointerClickEvent>, IPointerClickHandler
    {
        /// <summary>
        /// 点击
        /// </summary>
        public event Action<PointerEventData> onPointerClick { add { pointerCallback += value; } remove { pointerCallback -= value; } }
        public void OnPointerClick(PointerEventData eventData) => pointerCallback?.Invoke(eventData);
    }

    /// <summary>
    /// UI点击事件合集
    /// 包含（长按，双击，单击，右键（移动端为长按））
    /// </summary>
    public class UIClickEvent : UIEventBase<UIClickEvent>, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        /// <summary>
        /// 双击间隔
        /// </summary>
        public float doubleClickInterval = 0.2f;
        /// <summary>
        /// 长按时间
        /// </summary>
        public float pressTime = 0.8f;
        public event Action<PointerEventData> onClick;
        public event Action<PointerEventData> onDoubleClick;
        /// <summary>
        /// 右键点击，在移动端无效
        /// </summary>
        public event Action<PointerEventData> onRightClick;
        public event Action<PointerEventData> onPress;
        private float _doubleTempTime = 0;
        private float _pressTempTime = 0;
        private bool _isClick = false;
        private bool _isPress = false;
        private bool _isTriggerPress = false;
        private PointerEventData _cacheData;
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isTriggerPress)
            {
                _isTriggerPress = false;
                return;
            }
#if UNITY_EDITOR || UNITY_STANDALONE
            if (eventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Right)
            {
                onRightClick?.Invoke(eventData);
                return;
            }
#endif
            if (onDoubleClick == null)
            {
                onClick?.Invoke(eventData);
            }
            else
            {
                if (_isClick)
                {
                    _isClick = false;
                    if (_doubleTempTime > 0)
                    {
                        onDoubleClick?.Invoke(eventData);
                    }
                }
                else
                {
                    _isClick = true;
                    _doubleTempTime = doubleClickInterval;
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPress = true;
            _pressTempTime = pressTime;
            _cacheData = eventData;
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            _isPress = false;
        }

        void Update()
        {
            if (_isClick)
            {
                _doubleTempTime -= Time.deltaTime;
                if (_doubleTempTime < 0)
                {
                    _isClick = false;
                    onClick?.Invoke(_cacheData);
                }
            }
            if (_isPress)
            {
                _pressTempTime -= Time.deltaTime;
                if (_pressTempTime < 0)
                {
                    _isPress = false;
                    _isTriggerPress = true;
                    onPress?.Invoke(_cacheData);
                }
            }
        }
    }
}
