using System;
using Pigeon.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MycoMenuLib;

public static partial class ModUI
{
    public static ModInputField AddTextInput(RectTransform parent, string id, string label, string value, Action<string> onEndEdit, float width = 0f, float height = 0f)
    {
        var input = AddInput(parent, id, label, value, new InputFieldOptions(),
            null, width, height);

        if (input != null && onEndEdit != null)
            input.onEndEdit.AddListener(s => onEndEdit(s));

        return input;
    }

    public static ModInputField AddInput(RectTransform parent, string id, string label, string value, Action<string> onChanged, float width = 0f, float height = 0f)
    {
        return AddInput(parent, id, label, value, null, onChanged, width, height);
    }

    public static ModInputField AddInput(RectTransform parent, string id, string label, string value, InputFieldOptions fieldOptions = null, Action<string> onChanged = null, float width = 0f, float height = 0f)
    {
        EnsureEscapeGuard(parent);
        fieldOptions ??= new InputFieldOptions();

        if (width <= 0f)
        {
            var parentWidth = parent.rect.width;
            var labelWidth = Mathf.Max(0f, Mathf.Min(parentWidth - RowWidth - 12f, 700f));
            width = Mathf.Max(parentWidth - labelWidth - 24f, RowWidth);
        }

        if (height <= 0f)
        {
            if (fieldOptions.Height > 0f)
                height = fieldOptions.Height;
            else if (fieldOptions.Multiline)
                height = 120f;
            else
                height = RowHeight;
        }

        var row = UnityEngine.Object.Instantiate(ModResources.Instance.NormalButton, parent, false);

        row.gameObject.name = $"{id} [Input]";
        row.gameObject.SetActive(false);
        SetRowRect((RectTransform)row.transform, width, height);

        var nameLabel = AddRowLabel((RectTransform)row.transform, label);

        var button = row.GetComponent<DefaultButton>();
        var background = button != null ? button.MainGraphic : null;
        var buttonText = button != null ? button.secondaryGraphic : null;

        if (button != null)
            UnityEngine.Object.Destroy(button);

        var colorButton = row.GetComponent<ColorButtonInstant>();
        if (colorButton != null)
            UnityEngine.Object.Destroy(colorButton);

        if (buttonText != null)
            UnityEngine.Object.Destroy(buttonText.gameObject);

        var bkg = row.transform.Find("BKG");
        var bevel = bkg != null ? bkg.GetComponent<BevelRect>() : null;
        if (bevel != null)
        {
            bevel.SetCorners(0f);
            bevel.color = new Color(0.12f, 0.12f, 0.12f, 1f);
        }

        if (bkg != null)
            background = bkg.GetComponent<Graphic>() ?? background;

        var outline = row.GetComponent<BevelRectOutline>();
        if (outline != null)
        {
            outline.SetCorners(0f);
            outline.color = Color.white;
        }

        if (background != null)
            background.raycastTarget = true;

        var showReveal = !fieldOptions.ReadOnly && fieldOptions.Type == ModInputType.Password && fieldOptions.ShowRevealButton;
        var viewport = CreateViewport(row.transform, showReveal);

        TextMeshProUGUI placeholder = null;
        if (!string.IsNullOrEmpty(fieldOptions.Placeholder))
            placeholder = CreateInputText(viewport, fieldOptions, true);

        var text = CreateInputText(viewport, fieldOptions, false);
        if (text == null)
        {
            UnityEngine.Object.Destroy(row.gameObject);
            return null;
        }

        var input = row.gameObject.AddComponent<ModInputField>();

        input.textViewport = viewport;
        input.lineType = fieldOptions.Multiline
            ? TMP_InputField.LineType.MultiLineNewline
            : TMP_InputField.LineType.SingleLine;
        input.textComponent = text;
        input.placeholder = placeholder;
        input.targetGraphic = background;
        input.transition = Selectable.Transition.None;
        input.richText = fieldOptions.RichText;
        input.caretWidth = 2;
        input.customCaretColor = true;
        input.caretColor = text.color;
        input.selectionColor = new Color(1f, 1f, 1f, 0.25f);
        input.asteriskChar = fieldOptions.MaskCharacter;
        input.deleteWordWithCtrlBackspace = fieldOptions.DeleteWordWithCtrlBackspace;
        input.deleteWordWithCtrlDelete = fieldOptions.DeleteWordWithCtrlDelete;
        input.nameLabel = nameLabel;
        input.readOnly = fieldOptions.ReadOnly;

        ApplyInputType(input, fieldOptions, fieldOptions.Type != ModInputType.Password);
        input.characterLimit = fieldOptions.CharacterLimit;
        input.SetTextWithoutNotify(value ?? string.Empty);

        if (fieldOptions.ReadOnly)
        {
            input.caretWidth = 0;
            input.selectionColor = new Color(1f, 1f, 1f, 0.1f);

            text.color = new Color(1f, 1f, 1f, 0.35f);

            if (bevel != null)
                bevel.color = new Color(0.05f, 0.05f, 0.05f, 1f);

            if (outline != null)
                outline.color = new Color(1f, 1f, 1f, 0.25f);
        }

        if (onChanged != null)
            input.onValueChanged.AddListener(s => onChanged(s));

        input.onSelect.AddListener(_ => EscapeGuard.OnInputFocused(input));
        input.onDeselect.AddListener(_ => EscapeGuard.OnInputBlurred(input));

        if (showReveal)
            AddRevealButton((RectTransform)row.transform, input, fieldOptions);

        row.gameObject.SetActive(true);
        return input;
    }

    private static RectTransform CreateViewport(Transform row, bool reveal)
    {
        var viewport = new GameObject("Viewport",
            typeof(RectTransform), typeof(RectMask2D)).GetComponent<RectTransform>();

        viewport.SetParent(row, false);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(10f, 6f);
        viewport.offsetMax = new Vector2(reveal ? -78f : -10f, -6f);

        return viewport;
    }

    private static TextMeshProUGUI CreateInputText(RectTransform viewport, InputFieldOptions fieldOptions, bool placeholder)
    {
        var template = GetLabelTemplate();
        if (template == null)
        {
            Plugin.Logger.LogWarning("No TMP text template found for input field");
            return null;
        }

        var text = UnityEngine.Object.Instantiate(template, viewport, false);

        text.gameObject.name = placeholder ? "Placeholder" : "Text";
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        text.alignment = TextAlignmentOptions.Left;
        text.textWrappingMode = fieldOptions.Multiline
            ? TextWrappingModes.Normal
            : TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        text.fontSize = fieldOptions.FontSize > 0f ? fieldOptions.FontSize : 22f;
        text.richText = fieldOptions.RichText;
        text.text = placeholder ? fieldOptions.Placeholder : string.Empty;

        if (placeholder)
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0.5f);

        return text;
    }

    private static void ApplyInputType(ModInputField input, InputFieldOptions fieldOptions, bool revealed)
    {
        if (fieldOptions.Type == ModInputType.Password && !revealed)
        {
            input.contentType = TMP_InputField.ContentType.Password;
            input.characterValidation = TMP_InputField.CharacterValidation.None;
            input.inputType = TMP_InputField.InputType.Password;
            return;
        }

        switch (fieldOptions.Type)
        {
            case ModInputType.Integer:
                input.contentType = TMP_InputField.ContentType.IntegerNumber;
                input.characterValidation = TMP_InputField.CharacterValidation.Integer;
                break;
            case ModInputType.Decimal:
                input.contentType = TMP_InputField.ContentType.DecimalNumber;
                input.characterValidation = TMP_InputField.CharacterValidation.Decimal;
                break;
            case ModInputType.Alphanumeric:
                input.contentType = TMP_InputField.ContentType.Alphanumeric;
                input.characterValidation = TMP_InputField.CharacterValidation.Alphanumeric;
                break;
            case ModInputType.Name:
                input.contentType = TMP_InputField.ContentType.Name;
                input.characterValidation = TMP_InputField.CharacterValidation.Name;
                break;
            case ModInputType.Email:
                input.contentType = TMP_InputField.ContentType.EmailAddress;
                input.characterValidation = TMP_InputField.CharacterValidation.EmailAddress;
                break;
            default:
                input.contentType = TMP_InputField.ContentType.Standard;
                input.characterValidation = TMP_InputField.CharacterValidation.None;
                break;
        }

        input.inputType = TMP_InputField.InputType.Standard;
    }

    private static void AddRevealButton(RectTransform row, ModInputField input, InputFieldOptions fieldOptions)
    {
        var reveal = UnityEngine.Object.Instantiate(ModResources.Instance.NormalButton, row, false);

        reveal.gameObject.name = "Reveal";
        var rt = (RectTransform)reveal.transform;
        rt.anchorMin = new Vector2(1f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(60f, 30f);
        rt.anchoredPosition = new Vector2(-8f, 0f);

        var label = reveal.secondaryGraphic as TextMeshProUGUI;
        if (label != null)
        {
            label.text = "SHOW";
            label.fontSize = 16f;
        }

        reveal.OnClickUp.RemoveAllListeners();
        ClearPersistentCalls(reveal.OnClickUp);

        var visible = false;

        reveal.OnClickUp.AddListener(Toggle);
        return;

        void Toggle()
        {
            visible = !visible;

            var textValue = input.text;
            var caret = input.stringPosition;

            ApplyInputType(input, fieldOptions, visible);
            input.characterLimit = fieldOptions.CharacterLimit;
            input.SetTextWithoutNotify(textValue);
            input.stringPosition = caret;

            input.ForceLabelUpdate();

            if (label != null)
                label.text = visible ? "HIDE" : "SHOW";
        }
    }
}
