using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Dungeon.Core.Events;
using Dungeon.Core.Events.Inventory;
using Dungeon.Gameplay.Items;
using Dungeon.Visual.UI.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dungeon.Visual.UI.Inventory
{
    public class InventoryWindow : WindowContentBuilder, IGamePlayEventListener<InventoryChangedEvent>, IGamePlayEventListener<InventoryClearEvent>
    {
        private const int SLOT_COUNT = 25;

        [Header("Templates")]
        [SerializeField] private VisualTreeAsset _inventoryWindowTemplate;
        [SerializeField] private VisualTreeAsset _itemSlotTemplate;

        [Header("Starting Items")]
        [SerializeField] private List<ItemInstance> _startingItems;

        private readonly List<InventorySlot> _slots = new();

        public static InventoryWindow Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Places the item in the first empty inventory slot. Returns false if the
        // inventory is full. 
        public bool TryReturnItem(ItemInstance item)
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

        /// <summary>
        /// Try to drop an item in the inventory
        /// TODO: This grabs the first one that matches the type. This is HIGHLY error prone and will be redone when the runtime inventory video is done
        /// </summary>
        /// <param name="item">Item to find a match to</param>
        /// <returns></returns>
        public bool TryDropItem(ItemInstance item)
        {
            if (item == null)
            {
                return false;   
            }

            var slot = _slots.FirstOrDefault(x => x.Item.Data.Id == item.Data.Id)?.DropItem();

            return slot != null;
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

        public void OnGameplayEvent(InventoryChangedEvent evt)
        {
            if (evt.ChangeEvent == InventoryChangedEvent.ChangeEvents.Added) //add the items
            {
                for (int i = 0; i < evt.Quantity; i++)
                {
                    TryReturnItem(evt.Item);
                }
            }
            else //Drop the items
            {
                TryDropItem(evt.Item);
            }
        }

        /// <summary>
        /// This is intended to be used with the CheatCommands only
        /// </summary>
        public void OnGameplayEvent(InventoryClearEvent gameplayEvent)
        {
            foreach (var slot in _slots)
            {
                if (slot.Item != null)
                {
                    Debug.Log($"Item being dropped: {slot.Item.Data.ItemName}");
                    slot.DropItem();
                }
            }

        } 

        private void OnEnable()
        {
            GameplayEventBus.Register<InventoryChangedEvent>(this);
            GameplayEventBus.Register<InventoryClearEvent>(this);
        }
        private void OnDisable()
        {
            GameplayEventBus.Unregister<InventoryChangedEvent>(this);
            GameplayEventBus.Unregister<InventoryClearEvent>(this);
        }

    }
}
