using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.Serialization;

namespace BoatAttack
{

    public enum CustomScreenOparateType
    {
        Empty, Steering, Reset, Throttle
    }
    /// <summary>
    /// This sends input controls to the boat engine if 'Human'
    /// </summary>
    public class CustomOnScreenStick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {

        [SerializeField]
        private int _playerIndex;
        private void Start()
        {
            m_StartPos = ((RectTransform)transform).anchoredPosition;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null)
                throw new System.ArgumentNullException(nameof(eventData));

            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out m_PointerDownPos);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData == null)
                throw new System.ArgumentNullException(nameof(eventData));

            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out var position);
            var delta = position - m_PointerDownPos;

            delta = Vector2.ClampMagnitude(delta, movementRange);
            ((RectTransform)transform).anchoredPosition = m_StartPos + (Vector3)delta;

            var newPos = new Vector2(delta.x / movementRange, delta.y / movementRange);

            // trigger event
            ScreenControlEvent.Instance.RaiseEvent(_playerIndex, CustomScreenOparateType.Steering, newPos.x);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ((RectTransform)transform).anchoredPosition = m_StartPos;
            ScreenControlEvent.Instance.RaiseEvent(_playerIndex, CustomScreenOparateType.Steering, 0);
        }


        public float movementRange
        {
            get => m_MovementRange;
            set => m_MovementRange = value;
        }

        [FormerlySerializedAs("movementRange")]
        [SerializeField]
        private float m_MovementRange = 50;

        private Vector3 m_StartPos;
        private Vector2 m_PointerDownPos;
    }


    public class ScreenControlEvent
    {
        private static ScreenControlEvent _instance;
        public static ScreenControlEvent Instance
        {
            get
            {
                if (_instance == null) _instance = new ScreenControlEvent();
                return _instance;
            }
        }

        /// <summary>
        /// 定义事件参数类 type 0 steering  1 reset  2 trottle
        /// </summary>
        public class ScreenControlEventArgs : EventArgs
        {
            public readonly int playerIndex;
            public readonly float value;
            public readonly CustomScreenOparateType type;
            public ScreenControlEventArgs(int playerIndex, CustomScreenOparateType type, float value)
            {
                this.playerIndex = playerIndex;
                this.value = value;
                this.type = type;
            }
        }

        ///定义一个委托
        public delegate void ScreenOperateEventHandler(object sender, ScreenControlEventArgs e);
        ///用event关键字声明事件对象
        public event ScreenOperateEventHandler ScreenOperateEvent;

        //事件触发的方法
        protected void DoNotify(ScreenControlEventArgs e)
        {
            if (ScreenOperateEvent != null)
            {
                ScreenOperateEvent(this, e);
            }
        }

        //引发事件的方法
        public void RaiseEvent(int playerIndex, CustomScreenOparateType type, float value)
        {
            ScreenControlEventArgs e = new ScreenControlEventArgs(playerIndex, type, value);
            DoNotify(e);
        }
    }


    //监听事件类
    public class TestEventListener
    {
        //定义本地处理事件的方法，他与声明事件的delegate具有相同的参数和返回值类型 
        public void KeyPressed(object sender, ScreenControlEvent.ScreenControlEventArgs e)
        {
            Console.WriteLine("发送者：{0}，所按得健为：{1}", sender, e);
        }

        //订阅事件 
        public void Subscribe(ScreenControlEvent evenSource)
        {
            evenSource.ScreenOperateEvent += new ScreenControlEvent.ScreenOperateEventHandler(KeyPressed);
        }

        //取消订阅事件 
        public void UnSubscribe(ScreenControlEvent evenSource)
        {
            evenSource.ScreenOperateEvent -= new ScreenControlEvent.ScreenOperateEventHandler(KeyPressed);
        }

    }
}

