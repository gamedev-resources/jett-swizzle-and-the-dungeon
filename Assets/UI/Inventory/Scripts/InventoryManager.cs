using UnityEngine;

public class InventoryManager : MonoBehaviour, IGamePlayEventListener<ItemCollectedEvent>
{

    public void OnGameplayEvent(ItemCollectedEvent evt)
    {
        Debug.Log($"Inventory: Collected {evt.Item.ItemName} x{evt.Quantity}");
        
    }
    private void OnEnable() => GameplayEventBus.Register<ItemCollectedEvent>(this);
    private void OnDisable() => GameplayEventBus.Unregister<ItemCollectedEvent>(this);

}
