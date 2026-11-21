using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Pigeon.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MycoMenuLib;

public static partial class ModUI
{
    private const float RowWidth = 300f;
    private const float RowHeight = 40f;

    public static TextMeshProUGUI AddCategory(RectTransform parent, string label, float width = 0f, float height = 0f)
    {
        EnsureEscapeGuard(parent);

        var template = GetLabelTemplate();
        if (template == null) return null;

        var rowGo = new GameObject($"{label} [Category]", typeof(RectTransform));
        var row = (RectTransform)rowGo.transform;
        row.SetParent(parent, false);
        SetRowRect(row, width > 0f ? width : RowWidth, height > 0f ? height : RowHeight);

        var text = AddRowLabel(row, label);
        if (text == null)
        {
            UnityEngine.Object.Destroy(rowGo);
            return null;
        }

        text.fontStyle = FontStyles.Bold;
        text.fontSize = 22f;

        return text;
    }

    private static DotMatrix _dividerTemplate;

    public static void AddDivider(RectTransform parent)
    {
        if (_dividerTemplate == null)
        {
            _dividerTemplate = UnityEngine.Object
                .FindObjectsByType<DotMatrix>(FindObjectsInactive.Include)
                .FirstOrDefault();

            if (_dividerTemplate == null) return;
        }

        var labelWidth = Mathf.Min(parent.rect.width - RowWidth - 12f, 700f);

        var rowGo = new GameObject("Divider", typeof(RectTransform));
        var row = (RectTransform)rowGo.transform;
        row.SetParent(parent, false);
        row.anchorMin = new Vector2(1f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(1f, 1f);
        row.sizeDelta = new Vector2(labelWidth + 310f, 16f);
        row.anchoredPosition = Vector2.zero;

        var dotsGo = UnityEngine.Object.Instantiate(_dividerTemplate.gameObject, row, false);
        var dotsRt = (RectTransform)dotsGo.transform;
        dotsRt.anchorMin = Vector2.zero;
        dotsRt.anchorMax = Vector2.one;
        dotsRt.offsetMin = Vector2.zero;
        dotsRt.offsetMax = Vector2.zero;

        if (dotsGo.GetComponent<DotMatrix>() is not { } dots) return;
        dots.size = 2f;
        dots.spacing = 12f;
        dots.center = false;
        dots.raycastTarget = false;
        dots.color = new Color(1f, 1f, 1f, 0.25f);
    }

    public static GameObject AddRowAction(Component control, Sprite icon, int slot, Action onClick)
    {
        if (control == null || icon == null) return null;

        var row = (RectTransform)control.transform;
        var container = row.parent as RectTransform;
        if (container == null) return null;

        var x = Mathf.Max(GetLabelLeft(row) - 8f - slot * 40f,
            -(container.rect.width - row.rect.width) + 4f);

        var button = UnityEngine.Object.Instantiate(ModResources.Instance.NormalButton, row, false);

        button.gameObject.name = $"RowAction {slot}";
        button.gameObject.SetActive(false);

        var rt = (RectTransform)button.transform;
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(36f, 32f);
        rt.anchoredPosition = new Vector2(x, 0f);

        if (button.SecondaryGraphic is TextMeshProUGUI text)
            UnityEngine.Object.Destroy(text.gameObject);

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        var iconRt = (RectTransform)iconGo.transform;
        iconRt.SetParent(button.transform, false);
        iconRt.anchorMin = new Vector2(0.5f, 0.5f);
        iconRt.anchorMax = new Vector2(0.5f, 0.5f);
        iconRt.sizeDelta = new Vector2(22f, 22f);

        var image = iconGo.GetComponent<Image>();
        image.sprite = icon;
        image.raycastTarget = false;

        button.OnClickUp.RemoveAllListeners();
        button.OnClickUp.AddListener(() => onClick());

        return button.gameObject;
    }

    public static ToggleOption AddToggle(RectTransform parent, string id, string label, bool value, Action<bool> onChanged, float width = RowWidth, float height = RowHeight)
    {
        EnsureEscapeGuard(parent);

        var toggle = UnityEngine.Object.Instantiate(ModResources.Instance.ToggleOption, parent, false);

        toggle.gameObject.name = $"{id} [Toggle]";
        SetRowRect((RectTransform)toggle.transform, width, height);

        toggle.id = id;
        toggle.nameText.text = label;
        toggle.valueType = Option.ValueType.Bool;
        toggle.defaultValue = value ? 1f : 0f;
        toggle.minValue = 0f;
        toggle.maxValue = 1f;

        if (onChanged != null)
            toggle.applyValue.AddListener(v => onChanged(v > 0.5f));

        toggle.Initialize();
        KeepLabel(toggle, label);
        return toggle;
    }

    public static Option AddFloatSlider(RectTransform parent, string id, string label, float value, float min, float max, int digits, Action<float> onChanged, float width = RowWidth, float height = RowHeight)
    {
        return AddSlider(parent, id, label, value, min, max, false, digits,
            onChanged, width, height);
    }

    public static Option AddIntSlider(RectTransform parent, string id, string label, int value, int min, int max, Action<int> onChanged, float width = RowWidth, float height = RowHeight)
    {
        return AddSlider(parent, id, label, value, min, max, true, 0,
            v => onChanged?.Invoke(Mathf.RoundToInt(v)), width, height);
    }

    public static DropdownOption AddDropdown(RectTransform parent, string id, string label, string[] options, int index, Action<int> onChanged, float width = RowWidth, float height = RowHeight)
    {
        EnsureEscapeGuard(parent);

        var dropdownPrefab = GetDropdownTemplate();
        if (dropdownPrefab == null)
        {
            Plugin.Logger.LogWarning($"No DropdownOption template found for {id}");
            return null;
        }

        if (options == null || options.Length == 0)
        {
            Plugin.Logger.LogWarning($"No options to display for {id}");
            return null;
        }

        var dropdown = UnityEngine.Object.Instantiate(dropdownPrefab, parent, false);

        dropdown.gameObject.name = $"{id} [Dropdown]";
        dropdown.gameObject.SetActive(false);
        SetRowRect((RectTransform)dropdown.transform, width, height);

        dropdown.id = id;
        dropdown.nameText.text = label;
        dropdown.valueType = Option.ValueType.Int;
        dropdown.optionLabels = options;
        dropdown.localizeLabels = false;
        dropdown.isMipBudget = false;
        dropdown.minValue = 0f;
        dropdown.maxValue = options.Length - 1;
        dropdown.defaultValue = Mathf.Clamp(index, 0, options.Length - 1);
        dropdown.remapDisplayedValue = false;

        var scrollBar = dropdown.GetComponentInChildren<ScrollBar>(true);
        if (scrollBar != null && options.Length > 1)
            scrollBar.SetSnapPoints(options.Length, 0);

        if (onChanged != null)
            dropdown.applyValue.AddListener(v =>
                onChanged(Mathf.Clamp(Mathf.RoundToInt(v), 0, options.Length - 1)));

        dropdown.gameObject.SetActive(true);
        dropdown.Initialize();

        dropdown.nameText.text = label;
        KeepLabel(dropdown, label);

        return dropdown;
    }

    public static RebindOption AddKeybind<T>(RectTransform parent, string id, string label, T current, Func<KeyCode, KeyCode[], T> factory, Func<T, string> format, Action<T> onChanged, float width = RowWidth, float height = RowHeight)
    {
        EnsureEscapeGuard(parent);

        var rebindPrefab = GetRebindTemplate();
        if (rebindPrefab == null)
        {
            Plugin.Logger.LogWarning($"No RebindOption template found for {id}");
            return null;
        }

        var row = UnityEngine.Object.Instantiate(rebindPrefab, parent, false);

        row.gameObject.name = $"{id} [{typeof(T).Name}]";
        row.gameObject.SetActive(false);
        SetRowRect((RectTransform)row.transform, width, height);

        row.bindingIndexKeyboard = -1;
        row.bindingIndexGamepad = -1;

        if (row.actionNameText != null)
            row.actionNameText.text = label;
        if (row.bindingText != null)
        {
            row.bindingText.text = format(current);
            row.bindingText.enableAutoSizing = true;
            row.bindingText.fontSizeMax = row.bindingText.fontSize;
            row.bindingText.fontSizeMin = 8f;
            row.bindingText.overflowMode = TextOverflowModes.Ellipsis;
            row.bindingText.textWrappingMode = TextWrappingModes.NoWrap;
        }

        var hasValue = !EqualityComparer<T>.Default.Equals(current, default);

        var capture = row.gameObject.AddComponent<KeybindCapture>();
        capture.enabled = false;

        var button = row.GetComponent<DoubleDefaultButton>();
        if (button != null)
        {
            button.OnClickUp.RemoveAllListeners();
            ClearPersistentCalls(button.OnClickUp);
            button.OnClickUp.AddListener(StartCapture);
        }

        row.gameObject.SetActive(true);
        return row;

        void StartCapture()
        {
            capture.Begin(row.bindingText, hasValue, (key, modifiers) =>
            {
                var value = factory(key, modifiers);
                hasValue = !EqualityComparer<T>.Default.Equals(value, default);

                if (row.bindingText != null)
                    row.bindingText.text = format(value);
                onChanged?.Invoke(value);
            });
        }
    }

    private static Option AddSlider(RectTransform parent, string id, string label, float current, float min, float max, bool isInt, int digits, Action<float> onChanged, float width, float height)
    {
        EnsureEscapeGuard(parent);

        var slider = UnityEngine.Object.Instantiate(ModResources.Instance.SliderOption, parent, false);

        slider.gameObject.name = $"{id} [{(isInt ? "Int" : "Float")}]";
        SetRowRect((RectTransform)slider.transform, width, height);

        slider.id = id;
        slider.nameText.text = label;
        slider.valueType = isInt ? Option.ValueType.Int : Option.ValueType.Float;
        slider.defaultValue = current;
        slider.minValue = min;
        slider.maxValue = max;
        slider.remapDisplayedValue = false;
        slider.roundValueTextToInt = false;
        slider.roundDigits = digits;

        if (isInt)
        {
            var scrollBar = slider.GetComponentInChildren<ScrollBar>(true);
            var steps = Mathf.RoundToInt(max - min) + 1;
            if (scrollBar != null && steps > 1)
                scrollBar.SetSnapPoints(steps, 0);
        }

        if (onChanged != null)
            slider.applyValue.AddListener(v => onChanged(v));

        // The game's int formatter throws on negatives and drops their decimals
        if (slider.valueText != null)
            slider.applyValue.AddListener(v => slider.valueText.text = FormatSliderText(v, isInt, digits));

        slider.Initialize();
        KeepLabel(slider, label);
        return slider;
    }

    private static string FormatSliderText(float value, bool isInt, int digits)
    {
        if (isInt)
            return Mathf.RoundToInt(value).ToString(CultureInfo.InvariantCulture);

        var format = digits > 0 ? "0." + new string('#', digits) : "0";
        return value.ToString(format, CultureInfo.InvariantCulture);
    }
}
