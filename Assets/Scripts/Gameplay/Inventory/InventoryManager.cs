using System.Collections.Generic;
using Dungeon.Core.Events;
using Dungeon.Core.Events.Inventory;
using Dungeon.Gameplay.Items;
using UnityEngine;

namespace Dungeon.Gameplay.Inventory
{
    public class InventoryManager : MonoBehaviour, IGamePlayEventListener<InventoryChangedEvent>
    {
        [SerializeReference]
        public List<ItemInstance> items = new();

        public void OnGameplayEvent(InventoryChangedEvent evt)
        {
            switch (evt.ChangeEvent)
            {
                case InventoryChangedEvent.ChangeEvents.Added:
                    items.Add(evt.Item);
                    break;

                case InventoryChangedEvent.ChangeEvents.Removed:
                    items.Remove(evt.Item);
                    break;
            }

            Debug.Log($"Inventory Change: {evt.Item.Data.ItemName} x{evt.Quantity} was {evt.ChangeEvent}");
        }
        private void OnEnable() => GameplayEventBus.Register<InventoryChangedEvent>(this);
        private void OnDisable() => GameplayEventBus.Unregister<InventoryChangedEvent>(this);

    }
}
