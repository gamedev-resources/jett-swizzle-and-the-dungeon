using System;
using Dungeon.Config.Items;
using UnityEngine;

namespace Dungeon.Gameplay.Items
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
