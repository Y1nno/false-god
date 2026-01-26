using UnityEngine;
using System.Collections.Generic;

public class Subject
{
    private List<IObserver> observers = new List<IObserver>();

    // Attach an observer to the subject
    public void AttachObserver(IObserver observer)
    {
        observers.Add(observer);
    }

    // Detach an observer from the subject
    public void DetachObserver(IObserver observer)
    {
        observers.Remove(observer);
    }

    // Notify all observers of an event
    public void Notify(EventType eventType)
    {
        foreach (var observer in observers)
        {
            observer.OnNotify(this, eventType);
        }
    }


}
