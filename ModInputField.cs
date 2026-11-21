using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace MycoMenuLib;

public enum ModInputType
{
    Text,
    Integer,
    Decimal,
    Alphanumeric,
    Name,
    Email,
    Password
}

public class InputFieldOptions
{
    public ModInputType Type = ModInputType.Text;
    public bool Multiline;
    public bool ReadOnly;
    public float Height;
    public int CharacterLimit;
    public bool RichText;
    public string Placeholder;
    public bool ShowRevealButton = true;
    public char MaskCharacter = '•';
    public float FontSize;
    public bool DeleteWordWithCtrlBackspace = true;
    public bool DeleteWordWithCtrlDelete = true;
}

public class ModInputField : TMP_InputField
{
    internal TMP_Text nameLabel;
    internal bool deleteWordWithCtrlBackspace = true;
    internal bool deleteWordWithCtrlDelete = true;

    public override void OnUpdateSelected(BaseEventData eventData)
    {
        if (!isFocused || readOnly || !interactable)
        {
            base.OnUpdateSelected(eventData);
            return;
        }

        var keyboard = Keyboard.current;
        var ctrl = keyboard != null && keyboard.ctrlKey.isPressed;
        var backspace = ctrl && deleteWordWithCtrlBackspace &&
                        keyboard.backspaceKey.wasPressedThisFrame;
        var delete = ctrl && deleteWordWithCtrlDelete &&
                     keyboard.deleteKey.wasPressedThisFrame;
        var hadSelection = HasSelection;

        base.OnUpdateSelected(eventData);

        if (backspace)
            DeleteAfterBase(hadSelection, backwards: true);
        else if (delete)
            DeleteAfterBase(hadSelection, backwards: false);
    }

    private void DeleteAfterBase(bool hadSelection, bool backwards)
    {
        if (HasSelection)
        {
            RemoveSelection();
            return;
        }

        if (hadSelection) return;

        DeleteWord(backwards);
    }

    private void DeleteWord(bool backwards)
    {
        var value = text;
        var start = stringPosition;
        var end = stringPosition;

        if (backwards)
        {
            while (start > 0 && !IsWordChar(value[start - 1])) start--;
            while (start > 0 && IsWordChar(value[start - 1])) start--;
        }
        else
        {
            while (end < value.Length && !IsWordChar(value[end])) end++;
            while (end < value.Length && IsWordChar(value[end])) end++;
        }

        if (end > start)
            Remove(start, end);
    }

    private void RemoveSelection()
    {
        var start = Mathf.Min(selectionStringAnchorPosition, selectionStringFocusPosition);
        var end = Mathf.Max(selectionStringAnchorPosition, selectionStringFocusPosition);

        if (end > start)
            Remove(start, end);
    }

    private void Remove(int start, int end)
    {
        var newValue = text.Remove(start, end - start);

        SetTextWithoutNotify(newValue);
        stringPosition = start;

        onValueChanged.Invoke(newValue);
        ForceLabelUpdate();
    }

    private bool HasSelection => selectionStringAnchorPosition != selectionStringFocusPosition;

    private static bool IsWordChar(char value)
    {
        return char.IsLetterOrDigit(value) || value == '_';
    }
}
