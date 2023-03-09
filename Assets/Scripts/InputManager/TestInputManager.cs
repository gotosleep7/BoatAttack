using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.Users;

public class TestInputManager
{
    private static TestInputManager _instance;
    public static TestInputManager Instance
    {
        get
        {
            if (_instance == null) _instance = new TestInputManager();
            return _instance;
        }
    }

    public InputUserAndGamepad player1;
    public int player1Index = 0;
    public InputUserAndGamepad player2;
    public int player2Index = 1;
    Type gamepadType = typeof(UnityEngine.InputSystem.Gamepad);



    private TestInputManager()
    {
        player1 = new InputUserAndGamepad(InputUser.CreateUserWithoutPairedDevices());
        player2 = new InputUserAndGamepad(InputUser.CreateUserWithoutPairedDevices());
        Debug.Log($"Gamepad.all.Count=={Gamepad.all.Count}");
        if (Gamepad.all.Count > 0)
        {
            player1.TrySetDevice(Gamepad.all[0]);
            if (Gamepad.all.Count > 1)
            {
                player2.TrySetDevice(Gamepad.all[1]);
            }
        }

        InputSystem.onDeviceChange += OnDeviceChange;
    }
    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    private void Destroy()
    {
        player1.InputUserInfo.UnpairDevicesAndRemoveUser();
        player2.InputUserInfo.UnpairDevicesAndRemoveUser();
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange state)
    {
        Type t1 = device.GetType();

        if (!gamepadType.IsAssignableFrom(t1)) return;
        Debug.Log($"id={device.deviceId}---name={device.name}---type={t1}---state={state}");
        if (state == InputDeviceChange.Disconnected || state == InputDeviceChange.Removed)
        {
            if (player1.CheckDeviceById(device.deviceId))
            {
                player1.RemoveDevice();
            }
            else if (player2.CheckDeviceById(device.deviceId))
            {
                player2.RemoveDevice();
            }
        }
        else if (state == InputDeviceChange.Reconnected)
        {
            // bool isReconnect = state == InputDeviceChange.Reconnected;
            if (player1.Device == null)
            {
                player1.TryReconnectDevice((Gamepad)device);
            }
            else if (player2.Device == null)
            {
                player2.TryReconnectDevice((Gamepad)device);
            }
        }
        else if (state == InputDeviceChange.Added)
        {
            // bool isReconnect = state == InputDeviceChange.Reconnected;
            if (player1.Device == null)
            {
                player1.TrySetDevice((Gamepad)device);
            }
            else if (player2.Device == null)
            {
                player2.TrySetDevice((Gamepad)device);
            }
        }

    }
}


public class InputUserAndGamepad
{
    public int playerIndex;
    public InputUser InputUserInfo { get; set; }
    public Gamepad Device { get; set; }
    public int CurrentDeviceId { get; set; }


    private InputControls _currnetController;
    public InputControls CurrnetController
    {
        get
        {
            return _currnetController;
        }
        set
        {
            _currnetController = value;
            AssociateActions();
        }
    }

    public InputUserAndGamepad(InputUser inputUserInfo, Gamepad gamepad, int deviceId, int playerIndex)
    {
        InputUserInfo = inputUserInfo;
        Device = gamepad;
        CurrentDeviceId = deviceId;
        this.playerIndex = playerIndex;
    }
    public InputUserAndGamepad(InputUser inputUserInfo)
    {
        InputUserInfo = inputUserInfo;
        Device = null;
        CurrentDeviceId = -1;
        this.playerIndex = -1;
    }

    public void TrySetDevice(Gamepad gamepad)
    {
        if (Device != null) return;
        InputUser? v = InputUser.FindUserPairedToDevice(gamepad);
        if (v != null) return;
        Device = gamepad;
        CurrentDeviceId = Device.deviceId;
        InputUserInfo = InputUser.PerformPairingWithDevice(gamepad, InputUserInfo);
        AssociateActions();

    }

    public void TryReconnectDevice(Gamepad gamepad)
    {
        if (Device != null) return;
        // InputUser? v = InputUser.FindUserPairedToDevice(gamepad);
        // if (v != null) return;
        Device = gamepad;
        CurrentDeviceId = Device.deviceId;
        InputUserInfo = InputUser.PerformPairingWithDevice(gamepad, InputUserInfo);
        AssociateActions();

    }

    public void AssociateActions()
    {
        Debug.Log($"AssociateActions==playerIndex={playerIndex}");
        if (_currnetController == null) return;
        InputUserInfo.AssociateActionsWithUser(_currnetController);
        _currnetController.Enable();
    }
    public void RemoveDevice()
    {
        CurrentDeviceId = -1;
        Device = null;
        _currnetController.Disable();
    }
    public bool CheckDeviceById(int deviceid)
    {
        return CurrentDeviceId == deviceid;
    }
}

