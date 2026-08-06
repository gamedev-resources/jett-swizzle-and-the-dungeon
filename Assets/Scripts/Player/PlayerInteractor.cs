using Dungeon.Environment;
using Dungeon.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dungeon.Player
{
    // Tracks nearby Interactables registered by the objects themselves via trigger overlap.
    // The closest one becomes the current target. On Interact input the target's Interact() is called.
    // Will be refactored to use the event bus later (interactable focus/unfocus events).
    public class PlayerInteractor : MonoBehaviour
    {
        private PlayerControls _controls;
        private List<Interactable> _nearby = new List<Interactable>();
        private Interactable _current;

        private void Awake()
        {
            _controls = new PlayerControls();
        }

        private void OnEnable()
        {
            _controls.Player.Enable();
            _controls.Player.Interact.performed += OnInteract;
        }

        private void OnDisable()
        {
            _controls.Player.Interact.performed -= OnInteract;
            _controls.Player.Disable();
        }

        private void OnDestroy() => _controls.Dispose();

        /// <summary>Called by <see cref="Interactable.OnTriggerEnter"/> — adds to the tracking list.</summary>
        public void RegisterInteractable(Interactable interactable)
        {
            if (!_nearby.Contains(interactable))
            {
                _nearby.Add(interactable);
                RefreshCurrent();
            }
        }

        /// <summary>Called by <see cref="Interactable.OnTriggerExit"/> — removes from the tracking list.</summary>
        public void UnregisterInteractable(Interactable interactable)
        {
            _nearby.Remove(interactable);
            RefreshCurrent();
        }

        /// <summary>Picks the closest interactable and updates the prompt for the new target.</summary>
        private void RefreshCurrent()
        {
            // Purge destroyed entries (e.g. picked up and destroyed objects)
            for (int i = _nearby.Count - 1; i >= 0; i--)
            {
                if (_nearby[i] == null) _nearby.RemoveAt(i);
            }

            //nothing is near so clear out
            if (_nearby.Count == 0)
            {
                _current = null;
                InteractionPromptUI.Instance?.Hide();
                return;
            }

            // Scan for the closest
            Interactable closest = null;
            float minDist = float.MaxValue;
            Vector3 playerPos = transform.position;

            foreach (var interactable in _nearby)
            {
                float distance = Vector3.Distance(playerPos, interactable.transform.position);
                if (distance < minDist)
                {
                    minDist = distance;
                    closest = interactable;
                }
            }

            // Only update the prompt when the target actually changes
            if (closest != _current)
            {
                _current = closest;
                InteractionPromptUI.Instance?.ShowAt(_current.transform.position, _current.PromptText);
            }
        }

        /// <summary>Handles Interact input — hides prompt, runs interaction, then refreshes.</summary>
        private void OnInteract(InputAction.CallbackContext context)
        {
            if (_current == null || !_current.CanInteract(transform))
            {
                return;
            }

            // Clear the current target before Interact() runs so any RefreshCurrent during
            // the interaction won't re-select the about-to-be-destroyed object
            var target = _current;
            _current = null;
            _nearby.Remove(target);
            InteractionPromptUI.Instance?.Hide();

            target.Interact(transform);

            // Pick up any remaining nearby (e.g. another pickup in range)
            RefreshCurrent();
        }
    }
}
