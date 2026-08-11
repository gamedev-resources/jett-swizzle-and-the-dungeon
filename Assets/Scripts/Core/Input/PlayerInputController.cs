using System;
using Dungeon.Core.Events;
using Dungeon.Core.Events.Input;
using Dungeon.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dungeon.Core.Input
{

    public class PlayerInputController : MonoBehaviour
    {
        /// <summary>
        /// Raised with the current move axis. The canceled phase reports
        /// <see cref="Vector2.zero"/>, so subscribers can treat it as "go idle".
        /// </summary>
        public event Action<Vector2> OnMove;

        /// <summary>Raised with <c>true</c> when sprint is pressed and <c>false</c> on release.</summary>
        public event Action<bool> OnSprint;

        /// <summary>Raised once each time the attack action is performed.</summary>
        public event Action OnAttack;

        /// <summary>Raised once each time the interact action is performed.</summary>
        public event Action OnInteract;

        /// <summary>The one and only instance of the generated input wrapper.</summary>
        private PlayerControls _controls;

        private void Awake() =>  _controls = new PlayerControls();

        /// <summary>Enables both action maps and subscribes the input handlers.</summary>
        private void OnEnable()
        {
            _controls.Player.Enable();
            _controls.UI.Enable();

            WirePlayerActions();
            WireUIActions();
        }

        /// <summary>Unsubscribes the handlers and disables both action maps.</summary>
        private void OnDisable()
        {
            UnwirePlayerActions();
            UnwireUIActions();

            _controls.Player.Disable();
            _controls.UI.Disable();
        }

        /// <summary>Releases the input asset to avoid leaking its unmanaged state.</summary>
        private void OnDestroy() => _controls?.Dispose();

        /// <summary>Subscribes to the gameplay actions on the <c>Player</c> map.</summary>
        private void WirePlayerActions()
        {
            // Move listens to canceled as well so release reports a zero vector (idle).
            _controls.Player.Move.performed += HandleMove;
            _controls.Player.Move.canceled += HandleMove;
            _controls.Player.Sprint.started += HandleSprintStarted;
            _controls.Player.Sprint.canceled += HandleSprintCanceled;
            _controls.Player.Interact.performed += HandleInteract;
            _controls.Player.Attack.performed += HandleAttack;
        }

        /// <summary>Unsubscribes from the gameplay actions on the <c>Player</c> map.</summary>
        private void UnwirePlayerActions()
        {
            _controls.Player.Move.performed -= HandleMove;
            _controls.Player.Move.canceled -= HandleMove;
            _controls.Player.Sprint.started -= HandleSprintStarted;
            _controls.Player.Sprint.canceled -= HandleSprintCanceled;
            _controls.Player.Interact.performed -= HandleInteract;
            _controls.Player.Attack.performed -= HandleAttack;
        }

        /// <summary>Subscribes to the window shortcuts on the <c>UI</c> map.</summary>
        private void WireUIActions()
        {
            _controls.UI.ToggleInventory.performed += HandleToggleInventory;
            _controls.UI.ToggleEquipment.performed += HandleToggleEquipment;
            _controls.UI.CloseAll.performed += HandleCloseAll;
        }

        /// <summary>Unsubscribes from the window shortcuts on the <c>UI</c> map.</summary>
        private void UnwireUIActions()
        {
            _controls.UI.ToggleInventory.performed -= HandleToggleInventory;
            _controls.UI.ToggleEquipment.performed -= HandleToggleEquipment;
            _controls.UI.CloseAll.performed -= HandleCloseAll;
        }

        private void HandleMove(InputAction.CallbackContext context) => 
            OnMove?.Invoke(context.ReadValue<Vector2>());

        private void HandleSprintStarted(InputAction.CallbackContext context) => OnSprint?.Invoke(true);

        private void HandleSprintCanceled(InputAction.CallbackContext context) => OnSprint?.Invoke(false);

        private void HandleInteract(InputAction.CallbackContext context) => OnInteract?.Invoke();

        private void HandleAttack(InputAction.CallbackContext context) => OnAttack?.Invoke();

        private void HandleToggleInventory(InputAction.CallbackContext context) => 
            GameplayEventBus.Raise(new WindowToggleInputEvent(WindowAction.Inventory, context.phase));

        private void HandleToggleEquipment(InputAction.CallbackContext context) => 
            GameplayEventBus.Raise(new WindowToggleInputEvent(WindowAction.Equipment, context.phase));

        private void HandleCloseAll(InputAction.CallbackContext context) => 
            GameplayEventBus.Raise(new WindowToggleInputEvent(WindowAction.CloseAll, context.phase));
    }
}
