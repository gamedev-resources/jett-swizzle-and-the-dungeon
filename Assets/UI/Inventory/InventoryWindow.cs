using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryWindow : WindowContentBuilder
{
    private const int SLOT_COUNT = 25;

    [Header("Templates")]
    [SerializeField] private VisualTreeAsset _inventoryWindowTemplate;
    [SerializeField] private VisualTreeAsset _itemSlotTemplate;

    [Header("Starting Items")]
    [SerializeField] private List<ItemData> _startingItems;

    private readonly List<InventorySlot> _slots = new();

    public static InventoryWindow Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // Places the item in the first empty inventory slot. Returns false if the
    // inventory is full. 
    public bool TryReturnItem(ItemData item)
    {
        if (item == null) return false;

        foreach (var slot in _slots)
        {
            if (slot.Item == null)
            {
                slot.HoldItem(item);
                return true;
            }
        }

        return false;
    }

    public override void Build(GameWindow window)
    {
        if (_inventoryWindowTemplate == null || _itemSlotTemplate == null)
        {
            Debug.LogError("Inventory Window Template or Item Slot Template is not assigned.");
            return;
        }

        var contentArea = window.ContentArea;
        contentArea.Clear();
        _slots.Clear();

        // Clone the inventory layout into the window's content area
        var slotContainer = _inventoryWindowTemplate.Instantiate().ExtractRoot("slot-container");
        if (slotContainer == null)
        {
            return;
        }

        contentArea.Add(slotContainer);

        // Generate slots
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            var slot = new InventorySlot(_itemSlotTemplate);
            slotContainer.Add(slot);
            _slots.Add(slot);
        }

        // Populate starting items
        if (_startingItems == null)
        {
            return;
        }

        for (int i = 0; i < _startingItems.Count && i < _slots.Count; i++)
        {
            if (_startingItems[i] != null)
            {
                _slots[i].HoldItem(_startingItems[i]);
            }
        }
    }
}
