using UnityEngine;
using System.Collections.Generic;

public class Subject
{
    protected List<IObserver> observers = new List<IObserver>();

    // Attach an observer to the subject
    public void AttachObserver(IObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
            return;
        }
    }

    // Detach an observer from the subject
    public void DetachObserver(IObserver observer)
    {
        if (observers.Contains(observer))
        {
            observers.Remove(observer);
            return;
        }
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
