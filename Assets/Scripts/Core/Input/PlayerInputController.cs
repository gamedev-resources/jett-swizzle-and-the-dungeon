using Dungeon.Events;
using Dungeon.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dungeon.Core.Input
{
    /// <summary>
    /// The single owner of the <see cref="PlayerControls"/> input asset.
    /// Translates every raw Input System callback into a strongly-typed event on the
    /// <see cref="GameplayEventBus"/>, so gameplay and UI systems consume input without
    /// each allocating their own copy of the action asset.
    /// </summary>
    /// <remarks>
    /// Handlers are named methods rather than lambdas so they can be unsubscribed in
    /// <see cref="OnDisable"/>; a lambda would create a new delegate instance on every
    /// wire-up and leak a subscription each time the component is re-enabled.
    /// </remarks>
    public class PlayerInputController : MonoBehaviour
    {
        /// <summary>The one and only instance of the generated input wrapper.</summary>
        private PlayerControls _controls;

        /// <summary>
        /// Allocates the input asset before any other lifecycle method runs, so
        /// <see cref="OnEnable"/> can safely enable and wire the action maps.
        /// </summary>
        private void Awake()
        {
            _controls = new PlayerControls();
        }

        /// <summary>Enables both action maps and subscribes the event-raising handlers.</summary>
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
        private void OnDestroy()
        {
            _controls?.Dispose();
        }

        /// <summary>Subscribes to the gameplay actions on the <c>Player</c> map.</summary>
        private void WirePlayerActions()
        {
            // Move listens to canceled as well so release raises a zero vector (idle).
            _controls.Player.Move.performed += OnMove;
            _controls.Player.Move.canceled += OnMove;
            _controls.Player.Sprint.started += OnSprintStarted;
            _controls.Player.Sprint.canceled += OnSprintCanceled;
            _controls.Player.Interact.performed += OnInteract;
            _controls.Player.Attack.performed += OnAttack;
        }

        /// <summary>Unsubscribes from the gameplay actions on the <c>Player</c> map.</summary>
        private void UnwirePlayerActions()
        {
            _controls.Player.Move.performed -= OnMove;
            _controls.Player.Move.canceled -= OnMove;
            _controls.Player.Sprint.started -= OnSprintStarted;
            _controls.Player.Sprint.canceled -= OnSprintCanceled;
            _controls.Player.Interact.performed -= OnInteract;
            _controls.Player.Attack.performed -= OnAttack;
        }

        /// <summary>Subscribes to the window shortcuts on the <c>UI</c> map.</summary>
        private void WireUIActions()
        {
            _controls.UI.ToggleInventory.performed += OnToggleInventory;
            _controls.UI.ToggleEquipment.performed += OnToggleEquipment;
            _controls.UI.CloseAll.performed += OnCloseAll;
        }

        /// <summary>Unsubscribes from the window shortcuts on the <c>UI</c> map.</summary>
        private void UnwireUIActions()
        {
            _controls.UI.ToggleInventory.performed -= OnToggleInventory;
            _controls.UI.ToggleEquipment.performed -= OnToggleEquipment;
            _controls.UI.CloseAll.performed -= OnCloseAll;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            GameplayEventBus.Raise(new MoveInputEvent(context.ReadValue<Vector2>(), context.phase));
        }

        private void OnSprintStarted(InputAction.CallbackContext context)
        {
            GameplayEventBus.Raise(new SprintInputEvent(true, context.phase));
        }

        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            GameplayEventBus.Raise(new SprintInputEvent(false, context.phase));
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            GameplayEventBus.Raise(new InteractInputEvent(context.phase));
        }

        private void OnAttack(InputAction.CallbackContext context)
        {
            GameplayEventBus.Raise(new AttackInputEvent(context.phase));
        }

        private void OnToggleInventory(InputAction.CallbackContext context)
        {
            GameplayEventBus.Raise(new WindowToggleInputEvent(WindowAction.Inventory, context.phase));
        }

        private void OnToggleEquipment(InputAction.CallbackContext context)
        {
            GameplayEventBus.Raise(new WindowToggleInputEvent(WindowAction.Equipment, context.phase));
        }

        private void OnCloseAll(InputAction.CallbackContext context)
        {
            GameplayEventBus.Raise(new WindowToggleInputEvent(WindowAction.CloseAll, context.phase));
        }
    }
}
