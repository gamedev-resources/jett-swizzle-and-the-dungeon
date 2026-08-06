using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Manages creation, toggling, and persistence of all game windows.
/// Provides a full-screen container for windows and handles position save/restore via PlayerPrefs.
/// </summary>
public class WindowManager : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private VisualTreeAsset _gameWindowTemplate;
    [Header("Shared Overlays")]
    [SerializeField] private StyleSheet _dragDropStyleSheet;
    [SerializeField] private VisualTreeAsset _itemTooltipTemplate;

    private VisualElement _windowContainer;
    private readonly Dictionary<string, GameWindow> _windows = new();
    private SharedUIOverlayHost _sharedUIOverlayHost;

    private void Awake()
    {
        if (_uiDocument == null && !TryGetComponent(out _uiDocument))
        {
            Debug.LogError("WindowManager requires a UIDocument reference.");
            enabled = false;
            return;
        }

        if (_gameWindowTemplate == null)
        {
            Debug.LogError("WindowManager requires a Game Window template.");
            enabled = false;
            return;
        }

        var root = _uiDocument.rootVisualElement;

        // Full-screen container for all windows, ignores clicks so they pass through
        _windowContainer = new VisualElement() 
        { 
            name = "window-container", 
            pickingMode = PickingMode.Ignore,
            style =
            {
                position = Position.Absolute,
                width = Length.Percent(100),
                height = Length.Percent(100)
            }
        };

        root.Add(_windowContainer);
    }

    public void InitializeSharedOverlays()
    {
        if (_sharedUIOverlayHost != null)
        {
            return;
        }

        _sharedUIOverlayHost = new SharedUIOverlayHost(_uiDocument.rootVisualElement, _dragDropStyleSheet, _itemTooltipTemplate);
        _sharedUIOverlayHost.Initialize();
    }

    /// <summary>
    /// Tries to fetch a window. If it can't, it will create a new window, restoring its position from PlayerPrefs if available.
    /// </summary>
    /// <param name="id">Unique identifier used for persistence and toggle lookups.</param>
    /// <param name="title">Display title shown in the window's title bar.</param>
    /// <param name="defaultPosition">Fallback position if no saved position exists.</param>
    public GameWindow GetOrCreateWindow(string id, string title, Vector2 defaultPosition)
    {
        if (_windows.TryGetValue(id, out var existingWindow))
        {
            return existingWindow;
        }

        float x = PlayerPrefs.GetFloat($"window_{id}_x", defaultPosition.x);
        float y = PlayerPrefs.GetFloat($"window_{id}_y", defaultPosition.y);

        var window = new GameWindow(_gameWindowTemplate, _windowContainer, title);
        window.SetPosition(x, y);

        _windows[id] = window;
        return window;
    }

    /// <summary>
    /// Toggles a window's visibility. Saves position when hiding.
    /// </summary>
    public void ToggleWindow(string id)
    {
        if (!_windows.TryGetValue(id, out var window))
        {
            Debug.LogError($"Window '{id}' has not been created.");
            return;
        }

        if (window.IsVisible)
        {
            SaveWindowPosition(id, window);
            window.Hide();
        }
        else
        {
            window.Show();
        }
    }

    /// <summary>
    /// Hides all visible windows, saving each one's position.
    /// </summary>
    public void CloseAllWindows()
    {
        foreach (var kvp in _windows)
        {
            if (kvp.Value.IsVisible)
            {
                SaveWindowPosition(kvp.Key, kvp.Value);
                kvp.Value.Hide();
            }
        }
    }

    /// <summary>
    /// Persists a single window's position to PlayerPrefs.
    /// </summary>
    private void SaveWindowPosition(string id, GameWindow window)
    {
        var pos = window.GetPosition();
        PlayerPrefs.SetFloat($"window_{id}_x", pos.x);
        PlayerPrefs.SetFloat($"window_{id}_y", pos.y);
    }

    /// <summary>
    /// Saves all visible window positions and flushes PlayerPrefs to disk.
    /// </summary>
    private void SaveAllPositions()
    {
        foreach (var kvp in _windows)
        {
            if (kvp.Value.IsVisible)
                SaveWindowPosition(kvp.Key, kvp.Value);
        }
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        SaveAllPositions();
    }
}