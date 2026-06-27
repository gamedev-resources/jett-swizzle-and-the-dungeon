using Dungeon.Player;
using UnityEngine;

/// <summary>
/// Handles keyboard input to toggle individual windows or close all windows.
/// Reads input from the <c>UI</c> action map of the shared <see cref="PlayerControls"/> asset,
/// keeping all bindings in one place alongside the <c>Player</c> map.
/// </summary>
public class UIController : MonoBehaviour
{
    [SerializeField] private WindowManager _windowManager;
    [SerializeField] private InventoryWindow _inventoryWindow;
    [SerializeField] private EquipmentWindow _equipmentWindow;

    /// <summary>
    /// Shared input-action wrapper; owns the lifetime of the underlying asset instance.
    /// </summary>
    private PlayerControls _controls;

    /// <summary>
    /// Allocates the <see cref="PlayerControls"/> instance before any other lifecycle
    /// methods run, so <see cref="OnEnable"/> can safely enable the action map.
    /// </summary>
    private void Awake()
    {
        _controls = new PlayerControls();
    }

    /// <summary>
    /// Creates the default inventory and equipment windows at their initial positions.
    /// </summary>
    private void Start()
    {
        var inventoryWindow = _windowManager.CreateWindow("inventory", "INVENTORY", new Vector2(50, 50));
        _inventoryWindow.BuildInventory(inventoryWindow.ContentArea);

        var equipmentWindow = _windowManager.CreateWindow("equipment", "EQUIPMENT", new Vector2(200, 100));
        if (_equipmentWindow != null)
        {
            _equipmentWindow.BuildEquipment(equipmentWindow);
        }

        inventoryWindow.Hide();
        equipmentWindow.Hide();
    }

    /// <summary>
    /// Enables the <c>UI</c> action map so its actions begin listening for input.
    /// </summary>
    private void OnEnable()
    {
        _controls.UI.Enable();
    }

    /// <summary>
    /// Disables the <c>UI</c> action map to stop listening for input while inactive.
    /// </summary>
    private void OnDisable()
    {
        _controls.UI.Disable();
    }

    /// <summary>
    /// Releases the <see cref="PlayerControls"/> asset to avoid memory leaks.
    /// </summary>
    private void OnDestroy()
    {
        _controls.Dispose();
    }

    /// <summary>
    /// Polls the <c>UI</c> action map each frame and toggles the corresponding window
    /// or closes all windows when the mapped key is pressed.
    /// </summary>
    private void Update()
    {
        if (_controls.UI.ToggleInventory.WasPressedThisFrame())
        {
            _windowManager.ToggleWindow("inventory");
        }
        if (_controls.UI.ToggleEquipment.WasPressedThisFrame())
        {
            _windowManager.ToggleWindow("equipment");
        }
        if (_controls.UI.CloseAll.WasPressedThisFrame())
        {
            // Don't interrupt an active drag — let the user release the item first.
            if (ItemDragManipulator.IsDragging) return;
            _windowManager.CloseAllWindows();
        }
    }
}
