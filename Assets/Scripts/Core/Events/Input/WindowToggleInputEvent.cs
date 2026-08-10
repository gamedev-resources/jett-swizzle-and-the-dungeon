using UnityEngine.InputSystem;

namespace Dungeon.Events
{
    /// <summary>Which UI window request an input produced.</summary>
    public enum WindowAction
    {
        Inventory,
        Equipment,
        CloseAll
    }

    /// <summary>
    /// Raised when a UI window shortcut fires. Listeners should act on
    /// <see cref="InputActionPhase.Performed"/> only.
    /// </summary>
    public class WindowToggleInputEvent : IGameplayEvent
    {
        /// <summary>The window request this input represents.</summary>
        public WindowAction Action { get; }

        /// <summary>Phase of the underlying action that produced this event.</summary>
        public InputActionPhase Phase { get; }

        public WindowToggleInputEvent(WindowAction action, InputActionPhase phase)
        {
            Action = action;
            Phase = phase;
        }
    }
}
