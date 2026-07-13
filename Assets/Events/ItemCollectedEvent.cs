using UnityEngine;

public class ItemCollectedEvent : IGameplayEvent
{
    public ItemData Item { get; }
    public int Quantity { get; }
    public Vector3 Position { get; }

    public ItemCollectedEvent(ItemData item, Vector3 position, int quantity = 1)
    {
        Item = item;
        Quantity = quantity;
        Position = position;
    }
}