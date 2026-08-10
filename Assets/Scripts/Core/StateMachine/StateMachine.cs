using System;
using System.Collections.Generic;

namespace Dungeon.Enemy
{
    /// <summary>
    /// A plain C# state machine. It is deliberately not a MonoBehaviour: the
    /// owner decides when to tick it, which keeps it testable and reusable.
    /// Transitions are registered up front as from, to, and a condition, so the
    /// states themselves never need to know about each other.
    /// </summary>
    public class StateMachine
    {
        /// <summary>
        /// A single registered rule: while in From, move to To once Condition is true.
        /// </summary>
        private class Transition
        {
            public State From;
            public State To;
            public Func<bool> Condition;
        }

        private readonly List<Transition> _transitions = new List<Transition>();
        private State _current;

        /// <summary>The state currently running, or null before SetInitial is called.</summary>
        public State Current
        {
            get { return _current; }
        }

        /// <summary>
        /// The runtime type name of the current state, for example PatrolState.
        /// Used for the Inspector readout so the state is visible during play mode.
        /// </summary>
        public string CurrentStateName
        {
            get { return _current == null ? "None" : _current.GetType().Name; }
        }

        /// <summary>
        /// Sets the starting state and calls Enter on it. Call this once during setup.
        /// </summary>
        public void SetInitial(State state)
        {
            _current = state;

            if (_current != null)
            {
                _current.Enter();
            }
        }

        /// <summary>
        /// Registers a transition. While the machine is in <paramref name="from"/>,
        /// it moves to <paramref name="to"/> as soon as <paramref name="condition"/>
        /// returns true.
        /// </summary>
        public void At(State from, State to, Func<bool> condition)
        {
            Transition transition = new Transition();
            transition.From = from;
            transition.To = to;
            transition.Condition = condition;

            _transitions.Add(transition);
        }

        /// <summary>
        /// Evaluates transitions, switches state if one matches, then ticks the
        /// current state. Call this once per frame from the owning MonoBehaviour.
        /// </summary>
        public void Tick()
        {
            if (_current == null)
            {
                return;
            }

            // A plain for loop over the list rather than LINQ or foreach, because
            // this runs every frame for every enemy and we want zero allocation.
            for (int i = 0; i < _transitions.Count; i++)
            {
                Transition transition = _transitions[i];

                if (transition.From != _current)
                {
                    continue;
                }

                if (transition.Condition != null && transition.Condition())
                {
                    Change(transition.To);
                    break;
                }
            }

            if (_current != null)
            {
                _current.Tick();
            }
        }

        /// <summary>
        /// Switches to a new state immediately: Exit on the old one, then Enter on
        /// the new one. Re entering the state that is already current is ignored.
        /// </summary>
        public void Change(State next)
        {
            if (next == _current)
            {
                return;
            }

            if (_current != null)
            {
                _current.Exit();
            }

            _current = next;

            if (_current != null)
            {
                _current.Enter();
            }
        }
    }
}
