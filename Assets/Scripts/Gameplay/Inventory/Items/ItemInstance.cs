using System;
using UnityEngine;

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