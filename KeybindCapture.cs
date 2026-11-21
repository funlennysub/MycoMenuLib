using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MycoMenuLib;

internal class KeybindCapture : MonoBehaviour
{
    private static KeybindCapture _sActive;

    private TMP_Text _display;
    private Action<KeyCode, KeyCode[]> _onCaptured;
    private string _originalText;
    private bool _capturing;
    private bool _canClear;

    internal static bool IsCapturing => _sActive != null && _sActive._capturing;

    internal void Begin(TMP_Text display, bool canClear, Action<KeyCode, KeyCode[]> onCaptured)
    {
        if (_capturing) return;

        _sActive?.Cancel();
        _sActive = this;

        _display = display;
        _onCaptured = onCaptured;
        _canClear = canClear;
        _originalText = display != null ? display.text : null;

        if (display != null) display.text = "PRESS A KEY...";
        _capturing = true;
        enabled = true;
    }

    private void Update()
    {
        if (!_capturing) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            if (_canClear)
            {
                StopCapture();
                _onCaptured?.Invoke(KeyCode.None, Array.Empty<KeyCode>());
            }
            else
            {
                Cancel();
            }

            return;
        }

        foreach (var control in keyboard.allKeys)
        {
            if (!control.wasPressedThisFrame || IsModifier(control.keyCode)) continue;

            var key = ToKeyCode(control.keyCode);
            if (key == KeyCode.None) continue;

            var modifiers = GetModifiers(keyboard);
            StopCapture();

            _onCaptured?.Invoke(key, modifiers);
            return;
        }
    }

    private void OnDisable() => Cancel();

    private void OnDestroy() => Cancel();

    private void Cancel()
    {
        if (!_capturing) return;

        StopCapture();
        if (_display != null) _display.text = _originalText;
    }

    private void StopCapture()
    {
        _capturing = false;
        if (_sActive == this) _sActive = null;
        enabled = false;
    }

    private static bool IsModifier(Key key)
    {
        return key is Key.LeftShift or Key.RightShift
            or Key.LeftCtrl or Key.RightCtrl
            or Key.LeftAlt or Key.RightAlt
            or Key.LeftMeta or Key.RightMeta;
    }

    private static KeyCode[] GetModifiers(Keyboard keyboard)
    {
        var modifiers = new List<KeyCode>(3);

        if (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed)
            modifiers.Add(KeyCode.LeftControl);
        if (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed)
            modifiers.Add(KeyCode.LeftShift);
        if (keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed)
            modifiers.Add(KeyCode.LeftAlt);

        return modifiers.ToArray();
    }

    private static KeyCode ToKeyCode(Key key)
    {
        var name = key.ToString();

        if (name.StartsWith("Digit") && int.TryParse(name.Substring(5), out var digit))
            return (KeyCode)((int)KeyCode.Alpha0 + digit);
        if (name.StartsWith("Numpad") && int.TryParse(name.Substring(6), out var numpad))
            return (KeyCode)((int)KeyCode.Keypad0 + numpad);

        return key switch
        {
            Key.Enter => KeyCode.Return,
            Key.LeftCtrl => KeyCode.LeftControl,
            Key.RightCtrl => KeyCode.RightControl,
            Key.LeftMeta => KeyCode.LeftCommand,
            Key.RightMeta => KeyCode.RightCommand,
            Key.PrintScreen => KeyCode.Print,
            Key.ContextMenu => KeyCode.Menu,
            _ => Enum.TryParse<KeyCode>(name, true, out var code) ? code : KeyCode.None
        };
    }
}
