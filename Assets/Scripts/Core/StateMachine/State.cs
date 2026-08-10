namespace Dungeon.Enemy
{
    /// <summary>
    /// One behaviour the enemy can be in, such as patrolling or attacking.
    /// A state knows how to run itself and nothing else. It never decides which
    /// state comes next, so states stay independent of one another and can be
    /// added or removed without touching the others.
    ///
    /// The methods are virtual rather than abstract, so a state only overrides
    /// the parts it actually needs.
    /// </summary>
    public abstract class State
    {
        /// <summary>
        /// Runs once when the machine switches into this state. Use it for setup
        /// such as picking a destination, stopping the agent, or firing a trigger.
        /// </summary>
        public virtual void Enter() { }

        /// <summary>
        /// Runs every frame while this state is the current one.
        /// </summary>
        public virtual void Tick() { }

        /// <summary>
        /// Runs once when the machine switches out of this state. Use it to undo
        /// anything Enter set up so the next state starts from a clean slate.
        /// </summary>
        public virtual void Exit() { }
    }
}
