using System;
using System.Collections.Generic;
using UnityEngine;

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