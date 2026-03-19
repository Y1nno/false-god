using UnityEngine;

public class GameModule : Subject
{
    public virtual void AttachDefaultObservers()
    {
        // Override in derived classes to attach default observers
    }

    public virtual void Cleanup()
    {
        ClearObservers();
    }
}
