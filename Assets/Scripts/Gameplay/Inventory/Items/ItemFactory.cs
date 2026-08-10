using Dungeon.Config.Items;

namespace Dungeon.Gameplay.Inventory.Items
{
    public static class ItemFactory
    {
        public static ItemInstance CreateItem(ItemData data)
        {
            if (data == null) return null;
    
            switch (data.Category)
            {
                case ItemCategory.Head:
                case ItemCategory.Weapon:
                case ItemCategory.Shield:
                case ItemCategory.Accessory:
                    return new EquippableItem(data);
                case ItemCategory.Consumable:
                    return new ConsumableItem(data);
                default:
                    return new EquippableItem(data);
            }
        }
    }
}

