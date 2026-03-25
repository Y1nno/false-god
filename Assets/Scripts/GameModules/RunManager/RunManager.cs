using UnityEngine;


// Singleton class that manages the lifecycle of a run
public class RunManager : GameModule
{
    private static RunManager _instance;

    private static ManagerBox s_mb;

    private static RunEnder s_runEnder;

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

    private RunManager()
    {
    }

    public override void AttachDefaultObservers()
    {
        // none for now
    }


    // API Methods

    // Starts a new run by initializing all managers. If a save exists, it can optionally load it.
    public void StartNewRun(bool forceNew = true)
    {
        if (s_mb != null)
        {
            s_mb.CleanupAllManagers();
        }
        s_mb = new ManagerBox();
        s_mb.InitializeAllManagers();
        s_runEnder = new RunEnder();

        bool loaded = false;
        SaveManager sm = GetService<SaveManager>();
        if (!forceNew && sm != null && sm.HasRunSave())
        {
            sm.LoadRun();
            TextOutputter.Instance?.OutputText("Continuing previous run.");
            loaded = true;
        }
        else
        {
            TextOutputter.Instance?.OutputText("New run started.");
        }

        Notify(EventType.RunStart);

        // If not loaded, trigger the first encounter manually
        if (!loaded)
        {
            GetService<DungeonManager>()?.Notify(EventType.DungeonRoomAdvance);
        }
    }

    // Ends the current run and returns a summary
    public RunSummary EndRun()
    {
        RunSummary rs = new RunSummary();
        if (s_mb != null)
        {
            s_mb.CleanupAllManagers();
            s_mb = null;
        }
        TextOutputter.Instance?.OutputText("Run ended.");
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
        if (s_mb != null)
        {
            return s_mb.GetManager<T>();
        }
        return null;
    }
}
