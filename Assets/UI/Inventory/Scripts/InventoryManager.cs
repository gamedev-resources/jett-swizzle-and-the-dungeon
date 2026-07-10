using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple instances of InventoryManager found. Destroying.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void OnItemCollected(ItemData item, int quantity)
    {
        Debug.Log($"Inventory: Collected {item.ItemName} x{quantity}");
    }
}
