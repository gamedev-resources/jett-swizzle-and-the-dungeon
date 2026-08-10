

namespace Dungeon.Core.Events
{
    public interface IGamePlayEventListener<T> where T : IGameplayEvent
    {
        void OnGameplayEvent(T gameplayEvent);
    }
}


