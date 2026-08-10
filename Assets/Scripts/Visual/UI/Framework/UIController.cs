using Dungeon.Events;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Creates the gameplay windows and toggles them in response to window shortcuts.
/// Input arrives as <see cref="WindowToggleInputEvent"/> on the <see cref="GameplayEventBus"/>,
/// raised by the single input owner, so this controller neither owns nor polls an input asset.
/// </summary>
public class UIController : MonoBehaviour, IGamePlayEventListener<WindowToggleInputEvent>
{
    [SerializeField] private WindowManager _windowManager;
    [SerializeField] private InventoryWindow _inventoryWindow;
    [SerializeField] private EquipmentWindow _equipmentWindow;

    /// <summary>
    /// Creates the default inventory and equipment windows at their initial positions.
    /// </summary>
    private void Start()
    {
        _windowManager.InitializeSharedOverlays();

        InitializeWindow("inventory", "INVENTORY", new Vector2(50, 50), _inventoryWindow);
        InitializeWindow("equipment", "EQUIPMENT", new Vector2(200, 100), _equipmentWindow);
    }

    private void InitializeWindow(string id, string title, Vector2 defaultPosition, WindowContentBuilder builder)
    {
        if (builder == null)
        {
            Debug.LogError($"No window builder assigned for '{id}'.");
            return;
        }

        var window = _windowManager.GetOrCreateWindow(id, title, defaultPosition);
        builder.Build(window);
        window.Hide();
    }

    /// <summary>Subscribes to window shortcut input.</summary>
    private void OnEnable() => GameplayEventBus.Register<WindowToggleInputEvent>(this);

    /// <summary>Unsubscribes from window shortcut input.</summary>
    private void OnDisable() => GameplayEventBus.Unregister<WindowToggleInputEvent>(this);

    /// <summary>
    /// Toggles the requested window, or closes everything for
    /// <see cref="WindowAction.CloseAll"/>.
    /// </summary>
    public void OnGameplayEvent(WindowToggleInputEvent gameplayEvent)
    {
        if (gameplayEvent.Phase != InputActionPhase.Performed)
        {
            return;
        }

        switch (gameplayEvent.Action)
        {
            case WindowAction.Inventory:
                _windowManager.ToggleWindow("inventory");
                break;

            case WindowAction.Equipment:
                _windowManager.ToggleWindow("equipment");
                break;

            case WindowAction.CloseAll:
                // Don't interrupt an active drag — let the user release the item first.
                if (ItemDragManipulator.IsDragging) return;
                _windowManager.CloseAllWindows();
                break;
        }
    }
}
