using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Right-click and drag inside the target element to spin the character on its Y axis.
/// Mirrors the WindowDragManipulator (Part 1) and ItemDragManipulator (Part 4) pattern:
/// register pointer callbacks, capture the pointer on down, release on up.
/// </summary>
public class PreviewRotateManipulator : PointerManipulator
{
    private const float SENSITIVITY = 0.8f;

    private readonly Transform _character;
    private bool _isDragging;
    private int _capturedPointerId;

    public PreviewRotateManipulator(VisualElement target, Transform character)
    {
        this.target = target;
        _character = character;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        // 1 = right mouse in Unity pointer events
        if (evt.button != 1) return;
        if (_character == null) return;

        _isDragging = true;
        _capturedPointerId = evt.pointerId;
        target.CapturePointer(evt.pointerId);
        evt.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!_isDragging) return;
        if (_character == null) return;

        _character.Rotate(0f, -evt.deltaPosition.x * SENSITIVITY, 0f, Space.World);
        evt.StopPropagation();
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (!_isDragging || evt.button != 1) return;

        _isDragging = false;
        target.ReleasePointer(_capturedPointerId);
        evt.StopPropagation();
    }
}
