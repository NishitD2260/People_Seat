using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public delegate void Callback(object arg);

public class EventController
{
    private static readonly ConcurrentDictionary<GameEvent, SortedList<int, List<Callback>>> eventTable =
        new ConcurrentDictionary<GameEvent, SortedList<int, List<Callback>>>();

    public static void StartListening(GameEvent eventType, Callback handler, int priority = 5)
    {
        if (!eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] = new SortedList<int, List<Callback>>();
        }

        var priorityTable = eventTable[eventType];
        lock (priorityTable)
        {
            if (!priorityTable.ContainsKey(priority))
            {
                priorityTable[priority] = new List<Callback>();
            }

            priorityTable[priority].Add(handler);
        }
    }

    public static void StopListening(GameEvent eventType, Callback handler)
    {
        if (eventTable.TryGetValue(eventType, out var priorityTable))
        {
            lock (priorityTable)
            {
                foreach (var handlers in priorityTable.Values)
                {
                    handlers.Remove(handler);
                }
            }
        }
    }

    public static void TriggerEvent(GameEvent eventType, object arg = null)
    {
        if (eventTable.TryGetValue(eventType, out var priorityTable))
        {
            var handlersToInvoke = new List<Callback>();
            lock (priorityTable)
            {
                foreach (var handlers in priorityTable.Values)
                {
                    handlersToInvoke.AddRange(handlers);
                }
            }

            foreach (var handler in handlersToInvoke)
            {
                handler(arg);
            }
        }
    }
}