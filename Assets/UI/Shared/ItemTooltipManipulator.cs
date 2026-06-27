using UnityEngine.UIElements;

public class ItemTooltipManipulator : Manipulator
{
    // Set once by whichever window builds the tooltip. One shared instance
    // serves every manipulator across every window for the life of the app.
    public static ItemTooltip Tooltip { get; set; }

    private readonly InventorySlot _slot;

    public ItemTooltipManipulator(InventorySlot slot) => _slot = slot;

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
        target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
    }

    private void OnPointerEnter(PointerEnterEvent evt)
    {
        // suppress the tooltip while a drag is in flight, the ghost is enough visual noise
        if (ItemDragManipulator.IsDragging) return;
        if (_slot.Item == null) return;

        Tooltip?.Show(_slot.Item, evt.position);
    }

    private void OnPointerLeave(PointerLeaveEvent evt) => Tooltip?.Hide();

    // PointerDown hides the tooltip at the start of a drag. The cursor stays
    // over the source slot during the drag, so no PointerLeave fires. Without
    // this, the tooltip sticks until the cursor finally leaves the slot.
    private void OnPointerDown(PointerDownEvent evt)
    {
        if (evt.button == 0)
            Tooltip?.Hide();
    }
}