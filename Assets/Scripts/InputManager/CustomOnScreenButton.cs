using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.Serialization;

namespace BoatAttack
{
    /// <summary>
    /// This sends input controls to the boat engine if 'Human'
    /// </summary>
    public class CustomOnScreenButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private int _playerIndex;
        [SerializeField]
        private CustomScreenOparateType oparateType;


        public void OnPointerUp(PointerEventData eventData)
        {
            ScreenControlEvent.Instance.RaiseEvent(_playerIndex, oparateType, 0f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ScreenControlEvent.Instance.RaiseEvent(_playerIndex, oparateType, 1f);
        }
    }

}

