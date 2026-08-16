using Dungeon.Core.Events;
using Dungeon.Gameplay.Items;
using UnityEngine;

namespace Dungeon.Core.Events.Combat
{

    public class AttackHitEvent : IGameplayEvent
    {
        public Transform Source {get; } 
        public Transform Target {get; }

        public AttackHitEvent(Transform source, Transform target)
        {
            Source = source;
            Target = target;
        }
        
    }
}