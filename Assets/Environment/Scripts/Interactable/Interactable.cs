using Dungeon.Player;
using UnityEngine;

namespace Dungeon.Environment
{
    public abstract class Interactable : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField] private string _promptText = "Interact";
        [SerializeField] private bool _interactionEnabled = true;

        public virtual string PromptText => _promptText;
        public bool InteractionEnabled => _interactionEnabled && isActiveAndEnabled;
        public virtual bool CanInteract(Transform interactor) => InteractionEnabled;
        public abstract void Interact(Transform interactor);

        public void SetInteractionEnabled(bool enabled)
        {
            _interactionEnabled = enabled;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (other.TryGetComponent<PlayerInteractor>(out var pi))
                pi.RegisterInteractable(this);
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (other.TryGetComponent<PlayerInteractor>(out var pi))
                pi.UnregisterInteractable(this);
        }
    }
}
