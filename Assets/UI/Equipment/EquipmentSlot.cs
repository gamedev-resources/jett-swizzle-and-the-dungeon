using UnityEngine.UIElements;

[UxmlElement]
public partial class EquipmentSlot : InventorySlot
{
    public static event System.Action<ItemCategory, bool> EquipmentChanged;
    public EquipmentSlot() => RegisterCallback<PointerDownEvent>(OnRightClick);

    private void OnRightClick(PointerDownEvent evt)
    {
        if (evt.button != 1) return;
        if (Item == null) return;

        evt.StopPropagation();

        var item = DropItem();
        InventoryWindow.Instance.TryReturnItem(item);
    }

    public override void HoldItem(ItemData item)
    {
        base.HoldItem(item);
        if (item != null)
            EquipmentChanged?.Invoke(AllowedCategory, true);
    }

    public override ItemData DropItem()
    {
        var dropped = base.DropItem();
        if (dropped != null)
            EquipmentChanged?.Invoke(AllowedCategory, false);
            
        return dropped;
    }

    [UxmlAttribute]
    public ItemCategory AllowedCategory {get; set;}

    public override bool CanAccept(ItemCategory category) => category == AllowedCategory;
    
}
