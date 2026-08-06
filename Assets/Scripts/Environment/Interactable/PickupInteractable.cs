using UnityEngine;

namespace Dungeon.Environment
{
    public class PickupInteractable : Interactable
    {
        [Header("Key Item")]
        [SerializeField] private ItemData _itemData;

        [Tooltip("Destroy the object when picked up")]
        [SerializeField] private bool _destroyObject = true;

        [SerializeField, Min(1)] private int quantity = 1;

        public override string PromptText => _itemData == null ? "Pick up" : $"Pick up {_itemData.ItemName}";

        public override bool CanInteract(Transform interactor) => InteractionEnabled && _itemData != null;

        public override void Interact(Transform interactor)
        {
            if (!CanInteract(interactor))
            {
                Debug.LogWarning($"[KeyPickupInteractable] {name} cannot be interacted with.");
                return;
            }
            var runtimeItem = ItemFactory.CreateItem(_itemData);

            GameplayEventBus.Raise(new InventoryChangedEvent(InventoryChangedEvent.ChangeEvents.Added, 
               runtimeItem, transform.position, quantity));
                
            AudioManager.Instance.Play(AudioManager.SoundId.KeyPickup, transform.position);

            if (_destroyObject)
            {
                Destroy(gameObject);
            }
        }
    }
}
