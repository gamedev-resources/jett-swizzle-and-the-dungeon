using System.Buffers.Text;
using Dungeon.UI;
using UnityEngine;

namespace Dungeon.Environment
{
    /// <summary>
    /// Interaction-ready world pickup for items.
    /// </summary>
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
                Debug.LogWarning($"[KeyPickupInteractable] {name} has no ItemData assigned.");
                return;
            }

            Debug.Log($"[KeyPickupInteractable] Picked up {_itemData.ItemName} x{quantity}.");

            if (_destroyObject)
            {
                Destroy(gameObject);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.tag.Equals("Player"))
            {
                return;
            }

            InteractionPromptUI.Instance.ShowAt(transform.position, PromptText);
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.tag.Equals("Player"))
            {
                return;
            }

            InteractionPromptUI.Instance.Hide();
        }

    }
}
