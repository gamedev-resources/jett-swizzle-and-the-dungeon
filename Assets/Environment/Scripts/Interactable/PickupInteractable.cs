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

            GameplayEventBus.Raise(new ItemCollectedEvent(_itemData, transform.position, quantity));
            AudioManager.Instance.Play(AudioManager.SoundId.KeyPickup, transform.position);

            if (_destroyObject)
            {
                Destroy(gameObject);
            }
        }
    }
}
