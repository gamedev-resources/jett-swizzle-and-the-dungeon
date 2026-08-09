using UnityEngine.InputSystem;

namespace Dungeon.Events
{
    /// <summary>
    /// Raised when the sprint input is pressed or released. Sprint is a hold, so
    /// <see cref="IsSprinting"/> carries the new held state rather than a toggle.
    /// </summary>
    public class SprintInputEvent : IGameplayEvent
    {
        /// <summary>True while the sprint input is held, false once it is released.</summary>
        public bool IsSprinting { get; }

        /// <summary>Phase of the underlying action that produced this event.</summary>
        public InputActionPhase Phase { get; }

        public SprintInputEvent(bool isSprinting, InputActionPhase phase)
        {
            IsSprinting = isSprinting;
            Phase = phase;
        }
    }
}
