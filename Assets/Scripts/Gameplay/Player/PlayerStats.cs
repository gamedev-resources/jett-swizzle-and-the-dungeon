using UnityEngine;
using UnityEngine.Serialization;

public class PlayerStats : MonoBehaviour
{
    [FormerlySerializedAs("Name")]
    public string PlayerName;
    public float Health;
    public float Stamina;
    public float Mana;

    private void Start()
    {
        Debug.Log($"Player: {PlayerName} | Health: {Health} | Stamina: {Stamina} | Mana: {Mana}");
    }

}
