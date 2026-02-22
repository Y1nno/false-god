using UnityEngine;

public class CandleRite : Rite, IObserver
{
    private const int k_EncountersToTrigger = 4;
    private int _encounterCount = 0;
    private EncounterManager _encounterManager;

    public override string Description => $"Candle: Choose next encounter in {k_EncountersToTrigger - _encounterCount} rooms.";

    public CandleRite() : base("Candle", 5, RiteType.Candle)
    {
    }

    public override void OnEquip(PlayerManager player)
    {
        _encounterCount = 0;
        _encounterManager = RunManager.Instance.GetService<EncounterManager>();
        if (_encounterManager != null)
        {
            _encounterManager.AttachObserver(this);
        }
    }

    public override void OnUnequip(PlayerManager player)
    {
        if (_encounterManager != null)
        {
            _encounterManager.DetachObserver(this);
            _encounterManager = null;
        }
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (eventType == EventType.EncounterResolve)
        {
            _encounterCount++;
            if (_encounterCount >= k_EncountersToTrigger)
            {
                _encounterCount = 0;
                ForceCandleChoice();
            }
        }
    }

    private void ForceCandleChoice()
    {
        DungeonManager dm = RunManager.Instance.GetService<DungeonManager>();
        float difficulty = 1.0f; 
        if (dm != null)
        {
             // Calculate vague difficulty based on floor
             DungeonFloorData floorData = dm.GetCurrentDungeonFloorData();
             difficulty = floorData.Floor + floorData.Room * 1.5f;
        }
        
        _encounterManager.ForceNextEncounter(new CandleChoiceEncounter(difficulty));
        TextOutputter.Instance.OutputText("The Candle flickers... Your path clears.");
    }
}
