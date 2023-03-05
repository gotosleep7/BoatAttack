using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.Users;

public class TestInputManager : MonoBehaviour
{
    public static TestInputManager Instance;

    public InputUserAndGamepad player1;
    public int player1Index = 0;
    public InputUserAndGamepad player2;
    public int player2Index = 1;
    Type gamepadType = typeof(UnityEngine.InputSystem.Gamepad);
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(Instance);

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
    private void OnDestroy()
    {
        player1.InputUserInfo.UnpairDevicesAndRemoveUser();
        player2.InputUserInfo.UnpairDevicesAndRemoveUser();
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
        else if (state == InputDeviceChange.Added || state == InputDeviceChange.Reconnected)
        {
            player1.TrySetDevice((Gamepad)device);
            player2.TrySetDevice((Gamepad)device);
        }

    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }
}


public class InputUserAndGamepad
{
    public int playerIndex;
    public InputUser InputUserInfo { get; set; }
    public Gamepad Device { get; set; }
    public bool IsAvailable { get; set; }
    public List<int> DeviceIdList = new List<int>();

    public InputUserAndGamepad(InputUser inputUserInfo, Gamepad gamepad, int deviceId, bool isAvailable, int playerIndex)
    {
        IsAvailable = isAvailable;
        InputUserInfo = inputUserInfo;
        Device = gamepad;
        DeviceIdList.Add(deviceId);
        this.playerIndex = playerIndex;
    }
    public InputUserAndGamepad(InputUser inputUserInfo)
    {
        InputUserInfo = inputUserInfo;
        Device = null;
        this.playerIndex = -1;
        IsAvailable = false;
    }

    public void TrySetDevice(Gamepad gamepad)
    {
        if (Device != null) return;
        InputUser? v = InputUser.FindUserPairedToDevice(gamepad);
        if (v != null) return;
        Device = gamepad;
        DeviceIdList.Add(Device.deviceId);
        InputUserInfo = InputUser.PerformPairingWithDevice(gamepad, InputUserInfo);
        IsAvailable = true;
    }
    // public void ReConnentDevice(Gamepad gamepad)
    // {
    //     if (IsAvailable) return;

    //     InputUserInfo = InputUser.PerformPairingWithDevice(gamepad, InputUserInfo);
    //     Device = gamepad;
    //     IsAvailable = true;
    //     DeviceId = gamepad.deviceId;
    // }
    public void RemoveDevice()
    {
        InputUserInfo.UnpairDevice(Device);
        Device = null;
        IsAvailable = false;
    }
    public bool CheckDeviceById(int deviceid)
    {
        foreach (var id in DeviceIdList)
        {
            if (id == deviceid)
            {
                return true;
            }
        }
        return false;

    }
}

