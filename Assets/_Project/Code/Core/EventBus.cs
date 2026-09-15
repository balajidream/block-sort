using System;
using System.Collections.Generic;

namespace BlockSort.Core
{
    public static class EventBus<T>
    {
        static readonly List<Action<T>> Listeners = new();

        public static void Subscribe(Action<T> listener)
        {
            if (listener != null && !Listeners.Contains(listener))
            {
                Listeners.Add(listener);
            }
        }

        public static void Unsubscribe(Action<T> listener) => Listeners.Remove(listener);

        public static void Raise(T value)
        {
            for (var i = Listeners.Count - 1; i >= 0; i--)
            {
                Listeners[i]?.Invoke(value);
            }
        }
    }

    public enum GameState
    {
        Boot,
        MainMenu,
        LoadingLevel,
        Playing,
        LevelWon,
        Paused
    }
}
