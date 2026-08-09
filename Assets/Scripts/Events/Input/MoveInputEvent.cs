using UnityEngine;
using UnityEngine.InputSystem;

namespace Dungeon.Events
{
    /// <summary>
    /// Raised when the player's move axis changes. The <c>canceled</c> phase carries a
    /// zero <see cref="Value"/>, so listeners can treat every raise as the current state
    /// without needing to special-case release.
    /// </summary>
    public class MoveInputEvent : IGameplayEvent
    {
        /// <summary>Planar move axis, x = strafe, y = forward. Zero when the input is released.</summary>
        public Vector2 Value { get; }

        /// <summary>Phase of the underlying action that produced this event.</summary>
        public InputActionPhase Phase { get; }

        public MoveInputEvent(Vector2 value, InputActionPhase phase)
        {
            Value = value;
            Phase = phase;
        }
    }
}
