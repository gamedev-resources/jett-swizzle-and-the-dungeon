using UnityEngine;

public class EquippableItem : ItemInstance
{
    [SerializeField]
    private float _durability;

    public float Durability => _durability;

    public EquippableItem(ItemData data) : base(data)
    {
        _durability = 1f;
    }
}