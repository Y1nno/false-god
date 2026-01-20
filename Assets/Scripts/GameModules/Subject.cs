using UnityEngine;

public class Subject
{
    private List<Observer> observers = new List<Observer>();

    // Attach an observer to the subject
    public void AttachObserver(Observer observer)
    {
        observers.Add(observer);
    }

    // Detach an observer from the subject
    public void DetachObserver(Observer observer)
    {
        observers.Remove(observer);
    }

    // Notify all observers of an event
    public void Notify(string eventType)
    {
        foreach (var observer in observers)
        {
            observer.OnNotify(this, eventType);
        }
    }
}
