using UnityEngine;
using UnityEngine.Serialization;
using Dungeon.Core.Events;

namespace Dungeon.Gameplay.Player
{
    public class PlayerStats : MonoBehaviour
    {
        [FormerlySerializedAs("Name")]
        public string PlayerName;
        public float MaxHealth = 100f;
        public float Health;
        public float Stamina;
        public float Mana;

        private void Start()
        {
            Debug.Log($"Player: {PlayerName} | Health: {Health} | Stamina: {Stamina} | Mana: {Mana}");
        }

        public void Heal(float amount = 25f)
        {
            ApplyHealthChange(Health + amount);
        }

        public void Kill()
        {
            ApplyHealthChange(0f);
        }

        private void ApplyHealthChange(float next)
        {
            float previousHealth = Health;
            Health = Mathf.Clamp(next, 0f, MaxHealth);
            GameplayEventBus.Raise(new HealthChangedEvent(transform, previousHealth, Health, MaxHealth));
        }

    }
}
