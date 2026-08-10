using System;
using UnityEngine;
using Dungeon.Config.Save;

namespace Dungeon.Config.Items
{
    
    
    
    [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string Id {get;} = Guid.NewGuid().ToString();
        public string ItemName;
        public string Description;
        public Sprite Icon;
        public ItemCategory Category;
        public ItemRarity Rarity;
    
        public string RarityClass => Rarity == ItemRarity.Common
            ? ""
            : $"rarity-{Rarity.ToString().ToLower()}";
    
    
    }
}


