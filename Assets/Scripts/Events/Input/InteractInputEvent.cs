using UnityEngine.InputSystem;

namespace Dungeon.Events
{
    /// <summary>
    /// Raised when the interact input fires. Listeners should act on
    /// <see cref="InputActionPhase.Performed"/> only.
    /// </summary>
    public class InteractInputEvent : IGameplayEvent
    {
        /// <summary>Phase of the underlying action that produced this event.</summary>
        public InputActionPhase Phase { get; }

        public InteractInputEvent(InputActionPhase phase)
        {
            Phase = phase;
        }
    }
}
