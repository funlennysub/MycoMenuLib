using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MycoMenuLib;

public static partial class ModUI
{
    private static DropdownOption _sDropdownTemplate;
    private static RebindOption _sRebindTemplate;
    private static TextMeshProUGUI _sLabelTemplate;

    private static float GetLabelLeft(RectTransform row)
    {
        var parent = row.parent as RectTransform;
        if (parent == null) return -RowWidth;

        var baseWidth = Mathf.Min(parent.rect.width - RowWidth - 12f, 700f);
        return -10f - (RowWidth - row.rect.width + baseWidth);
    }

    private static TextMeshProUGUI AddRowLabel(RectTransform row, string label)
    {
        var template = GetLabelTemplate();
        if (template == null) return null;

        var text = Object.Instantiate(template, row, false);

        text.gameObject.name = "Name";
        text.text = label;
        text.alignment = TextAlignmentOptions.Left;
        text.enableAutoSizing = false;
        text.raycastTarget = false;

        var parent = row.parent as RectTransform;
        if (parent == null) return text;

        var width = -10f - GetLabelLeft(row);
        if (width > 0f)
            text.rectTransform.sizeDelta =
                new Vector2(width, text.rectTransform.sizeDelta.y);

        return text;
    }

    private static void EnsureEscapeGuard(RectTransform parent)
    {
        if (parent == null || parent.GetComponent<EscapeGuardDriver>() != null) return;

        parent.gameObject.AddComponent<EscapeGuardDriver>();
    }

    public static ConfigHoverInfo SetDescription(Component control, string description)
    {
        return SetDescription<ConfigHoverInfo>(control, description);
    }

    private static T SetDescription<T>(Component control, string description)
        where T : ConfigHoverInfo
    {
        if (control == null || string.IsNullOrWhiteSpace(description)) return null;

        var label = control switch
        {
            Option option => option.nameText,
            RebindOption rebind => rebind.actionNameText,
            ModInputField input => input.nameLabel,
            _ => null
        };

        if (label == null) return null;

        label.raycastTarget = true;

        var hover = label.GetComponent<T>();
        if (hover == null)
            hover = label.gameObject.AddComponent<T>();

        hover.description = description;
        return hover;
    }

    private static void KeepLabel(Option option, string label)
    {
        var fix = option.gameObject.AddComponent<OptionLabelOverride>();
        fix.Label = option.nameText;
        fix.DisplayName = label;
    }

    private static void SetRowRect(RectTransform rt, float width = RowWidth, float height = RowHeight)
    {
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = Vector2.zero;
    }

    private static DropdownOption GetDropdownTemplate()
    {
        if (_sDropdownTemplate == null)
        {
            _sDropdownTemplate = Object
                                     .FindObjectsByType<DropdownOption>(FindObjectsInactive.Include)
                                     .FirstOrDefault(IsSettingsStyleDropdown)
                                 ?? Resources.FindObjectsOfTypeAll<DropdownOption>()
                                     .FirstOrDefault(IsSettingsStyleDropdown)
                                 ?? Object
                                     .FindObjectsByType<DropdownOption>(FindObjectsInactive.Include)
                                     .FirstOrDefault(option => option != null);
        }

        return _sDropdownTemplate;
    }

    private static bool IsSettingsStyleDropdown(DropdownOption option)
    {
        return option != null &&
               option.valueText != null &&
               option.nameText != null &&
               option.nameText.rectTransform.anchorMin.x < 0.25f;
    }

    private static RebindOption GetRebindTemplate()
    {
        if (_sRebindTemplate == null)
        {
            _sRebindTemplate = Object
                                   .FindObjectsByType<RebindOption>(FindObjectsInactive.Include)
                                   .FirstOrDefault(IsUsableRebindTemplate)
                               ?? Resources.FindObjectsOfTypeAll<RebindOption>()
                                   .FirstOrDefault(IsUsableRebindTemplate);
        }

        return _sRebindTemplate;
    }

    private static bool IsUsableRebindTemplate(RebindOption option)
    {
        return option != null && option.inputAction != null && option.bindingIndexKeyboard >= 0;
    }

    private static TextMeshProUGUI GetLabelTemplate()
    {
        if (_sLabelTemplate == null && ModResources.Instance.ToggleOption != null)
            _sLabelTemplate = ModResources.Instance.ToggleOption.nameText;

        return _sLabelTemplate;
    }

    private static void ClearPersistentCalls(UnityEventBase evt)
    {
        var group = typeof(UnityEventBase)
            .GetField("m_PersistentCalls", System.Reflection.BindingFlags.Instance |
                                           System.Reflection.BindingFlags.NonPublic)
            ?.GetValue(evt);

        (group?.GetType()
            .GetField("m_Calls", System.Reflection.BindingFlags.Instance |
                                 System.Reflection.BindingFlags.NonPublic)
            ?.GetValue(group) as System.Collections.IList)?.Clear();
    }
}
