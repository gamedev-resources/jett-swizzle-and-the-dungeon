using UnityEngine;

namespace Dungeon.Core.Events
{

    public class HealthChangedEvent : IGameplayEvent
    {
        public Transform Source {get; } 
        public float PreviousHealth {get; }
        public float NewHealth {get; }
        public float MaxHealth {get; }
        public bool IsDead {get; }

        public HealthChangedEvent(Transform source, float previousHealth, float newHealth, float maxHealth)
        {
            Source = source;
            PreviousHealth = previousHealth;
            NewHealth = newHealth;
            MaxHealth = maxHealth;
            IsDead = newHealth <= 0;
        }
        
    }
}
