using Unity.Properties;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public string ItemName;
    public string Description;
    public Sprite Icon;
    public ItemCategory Category;
    public ItemRarity Rarity;

    public string RarityClass => Rarity == ItemRarity.Common
        ? ""
        : $"rarity-{Rarity.ToString().ToLower()}";


}