using UnityEngine;
using UnityEngine.UIElements;

public class ItemDragManipulator : PointerManipulator
{
    // Ghost: shared across all slots (only one drag at a time)
    private static VisualElement _ghost;
    private static Image _ghostIcon;
    private static VisualElement _ghostRarity;
    private static string _currentGhostRarityClass;

    public static bool IsDragging { get; private set; }

    private InventorySlot _sourceSlot;
    private ItemData _draggedItem;
    private int _capturedPointerId;

    private InventorySlot _highlightedSlot;

    public ItemDragManipulator(InventorySlot slot)
    {
        target = slot;
    }

    // --- Ghost Setup ---

    // Build the ghost once and park it on the panel root so it can float over every slot.
    public static void InitGhost(VisualElement panelRoot, StyleSheet ghostStyleSheet)
    {
        _ghost = new VisualElement();
        _ghost.name = "drag-ghost";
        _ghost.AddToClassList("drag-ghost");
        // ignore picking so the ghost never steals events from slots underneath
        _ghost.pickingMode = PickingMode.Ignore;

        if (ghostStyleSheet != null)
        {
            _ghost.styleSheets.Add(ghostStyleSheet);
        }

        _ghostIcon = new Image();
        _ghostIcon.AddToClassList("drag-ghost-icon");
        _ghostIcon.pickingMode = PickingMode.Ignore;
        _ghost.Add(_ghostIcon);

        _ghostRarity = new VisualElement();
        _ghostRarity.AddToClassList("drag-ghost-rarity");
        _ghostRarity.pickingMode = PickingMode.Ignore;
        _ghost.Add(_ghostRarity);

        panelRoot.Add(_ghost);
    }

    private void ShowGhost(ItemData item, Vector2 position)
    {
        _ghostIcon.sprite = item.Icon;

        // Apply rarity class so the ghost mirrors the slot's appearance
        _currentGhostRarityClass = item.RarityClass;
        
        if (!string.IsNullOrEmpty(_currentGhostRarityClass))
            _ghostRarity.AddToClassList(_currentGhostRarityClass);

        // -28 centers the 56px ghost on the cursor
        _ghost.style.translate = new Translate(position.x - 28, position.y - 28);
        _ghost.style.display = DisplayStyle.Flex;
    }

    private void UpdateGhostPosition(Vector2 position)
    {
        _ghost.style.translate = new Translate(position.x - 28, position.y - 28);
    }

    private static void HideGhost()
    {
        _ghost.style.display = DisplayStyle.None;
        _ghostIcon.sprite = null;

        if (!string.IsNullOrEmpty(_currentGhostRarityClass))
        {
            _ghostRarity.RemoveFromClassList(_currentGhostRarityClass);
            _currentGhostRarityClass = null;
        }
    }

    private void ClearHighlight()
    {
        if (_highlightedSlot != null)
        {
            _highlightedSlot.SetDropHighlight(false);
            _highlightedSlot = null;
        }
    }

    // Put the item back where it came from and tear down drag state.
    private void CancelDrag()
    {
        _sourceSlot.HoldItem(_draggedItem);
        _sourceSlot.RemoveFromClassList("drag-active");
        ClearHighlight();

        IsDragging = false;
        _draggedItem = null;
        _sourceSlot = null;

        HideGhost();
        target.ReleasePointer(_capturedPointerId);
    }

    // --- Callback Registration ---

    // KeyDown is in the mix so Escape can bail out mid-drag (target is focused on pickup)
    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        target.RegisterCallback<KeyDownEvent>(OnKeyDown);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
    }

    // --- Event Handlers ---

    private void OnPointerDown(PointerDownEvent evt)
    {
        // Right-click during drag: cancel
       if (evt.button != 0 && IsDragging)
       {
           CancelDrag();
           evt.StopPropagation();
           return;
       }

       if (evt.button != 0) return;

       var slot = (InventorySlot)target;
       if (slot.Item == null) return;

       // pull the item off the slot up front so the source visually empties immediately
       IsDragging = true;
       _sourceSlot = slot;
       _draggedItem = slot.DropItem();
       _sourceSlot.AddToClassList("drag-active");

       ShowGhost(_draggedItem, evt.position);

       // capture so we keep getting move/up even when the pointer leaves the slot
       target.CapturePointer(evt.pointerId);
       _capturedPointerId = evt.pointerId;
       // focus the slot so KeyDown (Escape) routes here
       target.Focus();
       evt.StopPropagation();
    }

    private InventorySlot FindSlotUnderPointer(Vector2 position)
   {
       // Pick can land on a child (icon, label), walk up to the slot itself
       var picked = target.panel.Pick(position);

       var current = picked;
       while (current != null)
       {
           if (current is InventorySlot slot)
               return slot;
           current = current.parent;
       }

       return null;
   }

    private void OnPointerMove(PointerMoveEvent evt)
    {
       if (!IsDragging) return;

       UpdateGhostPosition(evt.position);

        // Highlight the slot under the cursor
        var slotUnderPointer = FindSlotUnderPointer(evt.position);

        if (slotUnderPointer != _highlightedSlot)
        {
            ClearHighlight();

            if (slotUnderPointer != null && slotUnderPointer != _sourceSlot)
            {
                bool valid = slotUnderPointer.CanAccept(_draggedItem.Category);
                slotUnderPointer.SetDropHighlight(true, valid);
                _highlightedSlot = slotUnderPointer;
            }
        }

       evt.StopPropagation();
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
       if (!IsDragging || evt.button != 0) return;

       var targetSlot = FindSlotUnderPointer(evt.position);

       if (targetSlot != null && targetSlot != _sourceSlot && targetSlot.CanAccept(_draggedItem.Category))
       {
           if (targetSlot.Item != null)
           {
               // Swap: pull the target's item out before placing ours, then send it back to source
               var swappedItem = targetSlot.DropItem();
               targetSlot.HoldItem(_draggedItem);
               _sourceSlot.HoldItem(swappedItem);
           }
           else
           {
               // Place
               targetSlot.HoldItem(_draggedItem);
           }

           _sourceSlot.RemoveFromClassList("drag-active");
           ClearHighlight();

           IsDragging = false;
           _draggedItem = null;
           _sourceSlot = null;

           HideGhost();
           target.ReleasePointer(evt.pointerId);
       }
       else
       {
           // No valid target, or dropped on source: cancel
           ClearHighlight();
           CancelDrag();
       }
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        if (!IsDragging) return;

        if (evt.keyCode == KeyCode.Escape)
        {
            CancelDrag();
            evt.StopPropagation();
        }
    }
}
