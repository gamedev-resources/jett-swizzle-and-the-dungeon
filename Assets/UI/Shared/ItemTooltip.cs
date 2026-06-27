using UnityEngine;
using UnityEngine.UIElements;

public class ItemTooltip : VisualElement
{
    private VisualElement _container;
    private string _currentRarityClass;

    public ItemTooltip(VisualTreeAsset template)
    {
        pickingMode = PickingMode.Ignore;
        style.position = Position.Absolute;
        style.display = DisplayStyle.None;

        var root = template.Instantiate().ExtractRoot("ItemTooltip");
        Add(root);
        _container = root;

        // Tooltip is purely visual. Block it from receiving any pointer events.
        root.Query<VisualElement>().ForEach(el => el.pickingMode = PickingMode.Ignore);
    
    }

    public void Show(ItemData item, Vector2 position)
    {
        if (item == null) return;

        // Single assignment. Every binding declared in UXML resolves against this source.
        _container.dataSource = item;

        // swap rarity class so border/background colors match the item
        if (!string.IsNullOrEmpty(_currentRarityClass))
            _container.RemoveFromClassList(_currentRarityClass);

        _currentRarityClass = item.RarityClass;

        if (!string.IsNullOrEmpty(_currentRarityClass))
            _container.AddToClassList(_currentRarityClass);

        // nudge off the cursor so the tooltip doesn't sit directly under it
        style.translate = new Translate(position.x + 16, position.y + 16);
        style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        style.display = DisplayStyle.None;
        // drop the binding so the hidden tooltip doesn't keep a reference to the last item
        _container.dataSource = null;

        if (!string.IsNullOrEmpty(_currentRarityClass))
        {
            _container.RemoveFromClassList(_currentRarityClass);
            _currentRarityClass = null;
        }
    }
}
