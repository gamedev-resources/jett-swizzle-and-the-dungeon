using Dungeon.Config.Items;
using UnityEngine;

namespace Dungeon.Gameplay.Items
{
    public class ConsumableItem : ItemInstance
    {
        [SerializeField]
        private int _usesRemaining;

        public int UsesRemaining => _usesRemaining;

        public ConsumableItem(ItemData data) : base(data)
        {
            _usesRemaining = 2;
        }
    }
}
