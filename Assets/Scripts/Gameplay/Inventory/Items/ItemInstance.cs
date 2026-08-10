using System;
using UnityEngine;
using Dungeon.Config.Items;

namespace Dungeon.Gameplay.Inventory.Items
{
    
    
    
    [Serializable]
    public class ItemInstance
    {
        [SerializeField] private ItemData _data;
    
        public ItemData Data => _data;
    
        protected ItemInstance(ItemData data)
        {
            _data = data;
        }
    }
}

