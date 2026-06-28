using UnityEngine;
using UnityEngine.UIElements;

public sealed class SharedUIOverlayHost
{
    private readonly VisualElement _panelRoot;
    private readonly StyleSheet _dragDropStyleSheet;
    private readonly VisualTreeAsset _itemTooltipTemplate;
    private bool _isInitialized;

    public SharedUIOverlayHost(VisualElement panelRoot,StyleSheet dragDropStyleSheet, VisualTreeAsset itemTooltipTemplate)
    {
        _panelRoot = panelRoot;
        _dragDropStyleSheet = dragDropStyleSheet;
        _itemTooltipTemplate = itemTooltipTemplate;
    }

    public void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        if (_dragDropStyleSheet != null && !HasStyleSheet(_panelRoot, _dragDropStyleSheet))
        {
            _panelRoot.styleSheets.Add(_dragDropStyleSheet);
        }

        ItemDragManipulator.InitGhost(_panelRoot, null);

        if (_itemTooltipTemplate == null)
        {
            Debug.LogError("Item Tooltip Template is not assigned on the WindowManager.");
        }
        else if (ItemTooltipManipulator.Tooltip == null)
        {
            var tooltip = new ItemTooltip(_itemTooltipTemplate);
            _panelRoot.Add(tooltip);
            ItemTooltipManipulator.Tooltip = tooltip;
        }

        _isInitialized = true;
    }

    private static bool HasStyleSheet(VisualElement element, StyleSheet styleSheet)
    {
        for (int i = 0; i < element.styleSheets.count; i++)
        {
            if (element.styleSheets[i] == styleSheet)
            {
                return true;
            }
        }

        return false;
    }
}
