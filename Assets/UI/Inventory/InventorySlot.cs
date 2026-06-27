using System.Runtime.Versioning;
using System.Xml.Serialization;
using UnityEngine.UIElements;

public class InventorySlot : VisualElement
{
    private string _rarityClass = "";
    private VisualElement _slotRoot;
    private VisualElement _icon;
    private ItemData _item;
    public ItemData Item => _item;

    // Parameterless constructor so a [UxmlElement] subclass (EquipmentSlot) can be
    // instantiated from UXML. The template work moved into Build below.
    public InventorySlot() { }

    public virtual bool CanAccept(ItemCategory category) => true;

    public InventorySlot(VisualTreeAsset template)
    {
        Build(template);
    }

    // Builds the slot visual from the shared ItemSlot template. Lives outside the
    // constructor so UXML-instantiated slots can be initialized after the fact.
    public void Build(VisualTreeAsset template)
    {
        focusable = true;
        
        _slotRoot = template.Instantiate().ExtractRoot("ItemSlot");
        this.Add(_slotRoot);

        _icon = _slotRoot.Q<VisualElement>("Icon");

        this.AddManipulator(new ItemDragManipulator(this));
        this.AddManipulator(new ItemTooltipManipulator(this));
    }

    public virtual void HoldItem(ItemData item)
    {
        if (item == null) 
        {
            return;
        }

        ClearSlot();

        _item = item;
        _icon.style.backgroundImage = new StyleBackground(item.Icon);


        _rarityClass = item.RarityClass;
        _slotRoot.AddToClassList(_rarityClass);

    }

    public virtual ItemData DropItem()
    {
        if (_item == null) 
        {
            return null;
        }

        var droppedItem = _item;
        _item = null;

        ClearSlot();

        return droppedItem;
    }

    private void ClearSlot()
    {
        _icon.style.backgroundImage = StyleKeyword.None;

        _slotRoot.RemoveFromClassList(_rarityClass);
        _rarityClass = "";
    }

    public void SetDropHighlight(bool active, bool valid = true)
    {
        _slotRoot.EnableInClassList("drop-target", active && valid);
        _slotRoot.EnableInClassList("drop-target--invalid", active && !valid);
    }

}
