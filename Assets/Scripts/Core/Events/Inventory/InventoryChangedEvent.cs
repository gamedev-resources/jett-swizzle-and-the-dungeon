using UnityEngine;

namespace Dungeon.Events
{
    public class InventoryChangedEvent : IGameplayEvent
    {
        public enum ChangeEvents
        {
            Added,
            Removed
        }
        public ChangeEvents ChangeEvent {get; }
        public ItemInstance Item { get; }
        public int Quantity { get; }
        public Vector3 Position { get; }

        public InventoryChangedEvent(ChangeEvents changeEvent, ItemInstance item, Vector3 position, int quantity = 1)
        {
            ChangeEvent = changeEvent;
            Item = item;
            Quantity = quantity;
            Position = position;
        }
    }
}
