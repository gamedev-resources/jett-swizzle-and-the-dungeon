using UnityEngine;
using Dungeon.Player;

namespace Dungeon.Environment
{
    /// <summary>
    /// Base class for world objects the player can interact with.
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField] private string _promptText = "Interact";
        [SerializeField] private bool _interactionEnabled = true;


        /// <summary>
        /// Text shown by UI (or logs) when this object is in focus.
        /// </summary>
        public virtual string PromptText => _promptText;

        /// <summary>
        /// Returns true when interaction is globally enabled for this object.
        /// </summary>
        public bool InteractionEnabled => _interactionEnabled && isActiveAndEnabled;

        /// <summary>
        /// Override when interaction rules depend on player state (keys, quests, etc.).
        /// </summary>
        public virtual bool CanInteract(Transform interactor) => InteractionEnabled;

        /// <summary>
        /// Perform the interaction behavior.
        /// </summary>
        public abstract void Interact(Transform interactor);

        /// <summary>
        /// Enables or disables interactions at runtime.
        /// </summary>
        public void SetInteractionEnabled(bool enabled)
        {
            _interactionEnabled = enabled;
        }
    }
}
