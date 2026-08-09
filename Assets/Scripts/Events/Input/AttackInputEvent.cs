using UnityEngine.InputSystem;

namespace Dungeon.Events
{
    /// <summary>
    /// Raised when the attack input fires. Listeners should act on
    /// <see cref="InputActionPhase.Performed"/> only.
    /// </summary>
    public class AttackInputEvent : IGameplayEvent
    {
        /// <summary>Phase of the underlying action that produced this event.</summary>
        public InputActionPhase Phase { get; }

        public AttackInputEvent(InputActionPhase phase)
        {
            Phase = phase;
        }
    }
}
