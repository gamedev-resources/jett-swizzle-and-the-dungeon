using System;
using System.Collections.Generic;

/// <summary>
/// A lightweight gameplay event bus.
/// Systems can subscribe to specific event types and receive strongly-typed callbacks.
/// Changes to listeners made while an event is being processed are delayed until the
/// event finishes, preventing collection modification errors.
/// </summary>
public static class GameplayEventBus
{
    // Each event type gets its own listener list and pending changes.
    // Using separate storage per event type keeps events strongly typed and avoids
    // runtime type checks when sending events.
    private static class Storage<T> where T : IGameplayEvent
    {
        public static readonly HashSet<IGamePlayEventListener<T>> Listeners = new HashSet<IGamePlayEventListener<T>>();
        public static readonly List<IGamePlayEventListener<T>> PendingAdd = new List<IGamePlayEventListener<T>>();
        public static readonly List<IGamePlayEventListener<T>> PendingRemove = new List<IGamePlayEventListener<T>>();

        // Tracks how many times this event type is currently being processed.
        // Listener changes are applied only after the outermost raise finishes.
        public static int RaiseDepth;
    }

    /// <summary>
    /// Subscribe a listener to an event type.
    /// If the event is currently being processed, the subscription is applied afterward.
    /// </summary>
    public static void Register<T>(IGamePlayEventListener<T> listener) where T : IGameplayEvent
    {
        if (Storage<T>.RaiseDepth > 0)
        {
            // Wait until the current event finishes before changing the listener list.
            Storage<T>.PendingAdd.Add(listener);
        }
        else
        {
            // Add immediately when the event is not being processed.
            Storage<T>.Listeners.Add(listener);
        }
    }

    /// <summary>
    /// Remove a listener from an event type.
    /// If the event is currently being processed, the removal is applied afterward.
    /// </summary>
    public static void Unregister<T>(IGamePlayEventListener<T> listener) where T : IGameplayEvent
    {
        if (Storage<T>.RaiseDepth > 0)
        {
            // Wait until the current event finishes before changing the listener list.
            Storage<T>.PendingRemove.Add(listener);
        }
        else
        {
            // Remove immediately when the event is not being processed.
            Storage<T>.Listeners.Remove(listener);
        }
    }

    /// <summary>
    /// Sends an event to all listeners registered for that event type.
    /// Listeners can safely register or unregister while handling an event.
    /// Those changes are applied after all nested raises of this event finish.
    /// </summary>
    public static void Raise<T>(T gameplayEvent) where T : IGameplayEvent
    {
        var set = Storage<T>.Listeners;
        if (set.Count == 0)
        {
            return;
        }

        Storage<T>.RaiseDepth++;

        try
        {
            foreach (var listener in set)
            {
                try
                {
                    listener.OnGameplayEvent(gameplayEvent);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }
        }
        finally
        {
            Storage<T>.RaiseDepth--;

            if (Storage<T>.RaiseDepth == 0)
            {
                // Apply any listener changes that happened while the event was running.
                var pendingAdd = Storage<T>.PendingAdd;
                for (int i = 0; i < pendingAdd.Count; i++)
                {
                    set.Add(pendingAdd[i]);
                }
                pendingAdd.Clear();

                var pendingRemove = Storage<T>.PendingRemove;
                for (int i = 0; i < pendingRemove.Count; i++)
                {
                    set.Remove(pendingRemove[i]);
                }
                pendingRemove.Clear();
            }
        }
    }
}