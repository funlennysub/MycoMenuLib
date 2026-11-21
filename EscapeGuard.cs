using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace MycoMenuLib;

internal static class EscapeGuard
{
    private static int _activeDrivers;
    private static TMP_InputField _focusedInput;
    private static bool _inputFocused;
    private static bool _blockEscapeHold;
    private static float _lastInputFocusTime = -999f;
    private static float _lastCaptureTime = -999f;
    private static Func<bool> _pendingBlocker;
    
    internal static void RegisterPendingBlocker(Func<bool> tryBlock) =>
        _pendingBlocker = tryBlock;

    internal static bool ConsumePendingBlocker => _pendingBlocker != null && _pendingBlocker();

    public static void SetActive(bool active)
    {
        _activeDrivers += active ? 1 : -1;
        if (_activeDrivers < 0) _activeDrivers = 0;

        if (_activeDrivers == 0)
        {
            _focusedInput = null;
            _inputFocused = false;
            _blockEscapeHold = false;
            _lastInputFocusTime = -999f;
            _lastCaptureTime = -999f;
        }
    }

    public static void OnInputFocused(TMP_InputField input)
    {
        _focusedInput = input;
        _inputFocused = input != null;
        _lastInputFocusTime = Time.unscaledTime;
    }

    public static void OnInputBlurred(TMP_InputField input)
    {
        if (_focusedInput != input) return;

        _focusedInput = null;
        _inputFocused = false;
        _lastInputFocusTime = Time.unscaledTime;
    }

    public static void Tick()
    {
        if (_activeDrivers == 0) return;

        if (_focusedInput == null
            && !KeybindCapture.IsCapturing
            && !_blockEscapeHold
            && Time.unscaledTime - _lastInputFocusTime > 0.2f
            && Time.unscaledTime - _lastCaptureTime > 0.2f)
            return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (KeybindCapture.IsCapturing)
            _lastCaptureTime = Time.unscaledTime;

        if (_focusedInput != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            _focusedInput.DeactivateInputField();
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            _focusedInput = null;
            _inputFocused = false;
            _lastInputFocusTime = Time.unscaledTime;
        }

        if (_blockEscapeHold)
        {
            if (!keyboard.escapeKey.isPressed)
                _blockEscapeHold = false;
        }
        else if (keyboard.escapeKey.wasPressedThisFrame && ShouldBlockCore)
        {
            _blockEscapeHold = true;
        }
    }

    public static bool ShouldBlock
    {
        get
        {
            if (KeybindCapture.IsCapturing) return true;
            if (_inputFocused || Time.unscaledTime - _lastInputFocusTime < 0.15f) return true;

            if (_activeDrivers == 0) return false;

            return _blockEscapeHold || Time.unscaledTime - _lastCaptureTime < 0.15f;
        }
    }

    private static bool ShouldBlockCore
    {
        get
        {
            return KeybindCapture.IsCapturing
                || Time.unscaledTime - _lastCaptureTime < 0.15f
                || _inputFocused
                || Time.unscaledTime - _lastInputFocusTime < 0.15f;
        }
    }
}

internal class EscapeGuardDriver : MonoBehaviour
{
    private static EscapeGuardDriver _primary;

    private void OnEnable() => EscapeGuard.SetActive(true);

    private void OnDisable()
    {
        EscapeGuard.SetActive(false);
        if (_primary == this) _primary = null;
    }

    private void Update()
    {
        if (_primary == null) _primary = this;
        if (_primary == this) EscapeGuard.Tick();
    }
}
