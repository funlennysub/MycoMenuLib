using Pigeon;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MycoMenuLib;

public class ConfigHoverInfo : HoverInfo
{
    public string description;

    public bool customizeAnchor;
    public bool anchorToPointer;
    public Vector2 anchorOffset = Vector2.zero;
    public Vector2 anchorPivot = new(0.5f, 0f);
    public bool anchorWithMouse;

    private Vector2? _pointerPosition;

    public override string GetInfo(HoverInfoDisplay display)
    {
        return description ?? string.Empty;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        _pointerPosition = eventData.position;
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        _pointerPosition = null;
        base.OnPointerExit(eventData);
    }

    public override bool HasAnchor(out AnchorParams anchor)
    {
        if (!customizeAnchor)
            return base.HasAnchor(out anchor);

        if (!anchorToPointer || _pointerPosition == null)
        {
            if (!base.HasAnchor(out anchor))
                return false;

            anchor.Offset = anchorOffset;
            anchor.Pivot = anchorPivot;
            anchor.GamepadOnly = !anchorWithMouse;
            return true;
        }

        var canvas = GetComponentInParent<Canvas>();
        var camera = canvas != null ? canvas.worldCamera : null;
        var namePosition = RectTransformUtility.WorldToScreenPoint(camera, transform.position);

        anchor.Offset = ToParentPosition(_pointerPosition.Value, canvas) -
                        ToParentPosition(namePosition, canvas) +
                        anchorOffset;
        anchor.Pivot = new Vector2(0.5f, 0.5f);
        anchor.GamepadOnly = false;
        return true;
    }

    private static Vector2 ToParentPosition(Vector2 screenPosition, Canvas canvas)
    {
        var display = HoverInfoDisplay.Instance;
        var parent = display != null ? display.RectTransform.parent as RectTransform : null;

        if (parent == null)
            return screenPosition - new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        var camera = canvas != null ? canvas.worldCamera : null;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, screenPosition, camera, out var localPosition);

        return localPosition;
    }
}
