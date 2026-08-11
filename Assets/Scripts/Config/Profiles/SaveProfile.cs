using System;
using System.Collections.Generic;
using Dungeon.Config.Items;
using UnityEngine;

namespace Dungeon.Config.Profiles
{
    [CreateAssetMenu(fileName = "SaveProfile", menuName = "Game Data/Save Profile")]
    public class SaveProfile : ScriptableObject
    {
        public List<Item> Items;

        [Serializable]
        public struct Item
        {
            public ItemData ItemData;
            public int Amount;
        }
    }
}
