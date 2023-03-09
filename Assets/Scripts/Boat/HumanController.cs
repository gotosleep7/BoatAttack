using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace BoatAttack
{
    /// <summary>
    /// This sends input controls to the boat engine if 'Human'
    /// </summary>
    public class HumanController : BaseController
    {
        // private InputControls _controls;

        private float _throttle;
        private float _steering;

        private bool _paused;

        public InputUserAndGamepad inputUser;


        [SerializeField]
        private int _playerIndex;


        public int PlayerIndex { get { return _playerIndex; } set { _playerIndex = value; } }
        private void Awake()
        {
            // _controls = new InputControls();

        }
        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        private void Start()
        {
            if (PlayerIndex == 0)
            {
                inputUser = TestInputManager.Instance.player1;
                inputUser.playerIndex = 0;
            }
            if (PlayerIndex == 1)
            {
                inputUser = TestInputManager.Instance.player2;
                inputUser.playerIndex = 1;
            }
            inputUser.CurrnetController = new InputControls();

            inputUser.CurrnetController.BoatControls.Trottle.performed += (context) =>
            {
                if (IsMe(context.control.device.deviceId))
                {
                    _throttle = context.ReadValue<float>();
                }
            };
            inputUser.CurrnetController.BoatControls.Trottle.canceled += context =>
            {
                if (IsMe(context.control.device.deviceId)) _throttle = 0f;
            };

            inputUser.CurrnetController.BoatControls.Steering.performed += context =>
            {
                if (IsMe(context.control.device.deviceId))
                {
                    _steering = context.ReadValue<float>();
                }
            };
            inputUser.CurrnetController.BoatControls.Steering.canceled += context =>
            {
                if (IsMe(context.control.device.deviceId)) _steering = 0f;
            };

            inputUser.CurrnetController.BoatControls.Reset.performed += ResetBoat;
            inputUser.CurrnetController.BoatControls.Pause.performed += FreezeBoat;

            inputUser.CurrnetController.DebugControls.TimeOfDay.performed += SelectTime;
            ScreenControlEvent.Instance.ScreenOperateEvent += OnScreenOperateEvent;


        }

        private void OnScreenOperateEvent(object sender, ScreenControlEvent.ScreenControlEventArgs e)
        {
            if (e.playerIndex != PlayerIndex) return;
            switch (e.type)
            {
                case CustomScreenOparateType.Steering:
                    _steering = e.value;
                    break;
                case CustomScreenOparateType.Reset:
                    //reset
                    controller.ResetPosition();
                    break;
                case CustomScreenOparateType.Throttle:
                    _throttle = e.value;
                    break;
            }
        }

        public bool IsMe(int deviceId)
        {
            if (inputUser.playerIndex == PlayerIndex && inputUser.CheckDeviceById(deviceId))
            {
                return true;
            }
            return false;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            inputUser?.CurrnetController?.BoatControls.Enable();
        }

        private void OnDisable()
        {
            inputUser?.CurrnetController?.BoatControls.Disable();
        }

        private void ResetBoat(InputAction.CallbackContext context)
        {
            controller.ResetPosition();
        }

        private void FreezeBoat(InputAction.CallbackContext context)
        {
            _paused = !_paused;
            if (_paused)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }



        private void SelectTime(InputAction.CallbackContext context)
        {
            var value = context.ReadValue<float>();
            Debug.Log($"changing day time, input:{value}");
            DayNightController.SelectPreset(value);
        }

        void FixedUpdate()
        {
            engine.Accelerate(_throttle);
            engine.Turn(_steering);
        }



    }
}

