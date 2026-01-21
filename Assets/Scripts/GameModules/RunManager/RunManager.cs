using UnityEngine;


// Singleton class that manages the lifecycle of a run
public class RunManager
{
    private static RunManager _instance;

    private static ManagerBox mb;

    private static RunEnder _runEnder;

    public static RunManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new RunManager();
            }
            return _instance;
        }

        
    }

    private RunManager(){}


    // API Methods

    // Starts a new run by initializing all managers
    //TODO: Add start of core loop
    public void StartNewRun()
    {
        mb = new ManagerBox();
        mb.InitializeAllManagers();
        _runEnder = new RunEnder();
    }

    // Ends the current run and returns a summary
    public RunSummary EndRun()
    {
        RunSummary rs = new RunSummary();
        if (mb != null)
        {
            mb = null;
        }
        return rs;
    }

    // Retrieves the current run summary
    public RunSummary GetCurrentRunSummary()
    {
        RunSummary rs = new RunSummary();
        return rs;
    }

    // Retrieves a specific manager from the ManagerBox
    public T GetService<T>() where T : class
    {
        if (mb != null)
        {
            return mb.GetManager<T>();
        }
        return null;
    }

    
}
