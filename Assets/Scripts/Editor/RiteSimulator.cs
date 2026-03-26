using UnityEngine;
using UnityEditor;

public static class RiteSimulator
{
    [MenuItem("Debug/Simulate Rite Unlocks")]
    public static void SimulateRun()
    {
        if (!Application.isPlaying)
        {
            Debug.LogError("Failed: You must enter Play Mode first before running the simulation!");
            return;
        }

        Debug.Log("<color=cyan>--- STARTING AUTOMATED RITE TESTS ---</color>");
        
        var rm = RunManager.Instance.GetService<RiteManager>();
        var econ = RunManager.Instance.GetService<EconomyManager>();
        var inv = RunManager.Instance.GetService<InventoryManager>();
        
        Debug.Log("1. Simulating Resource Usage (Gold & Consumables)...");
        econ.AddGold(2000);
        econ.SpendGold(1000);
        
        for (int i = 0; i < 50; i++)
        {
            inv.UseOrEquipItem(new ConsumableInstance(ScriptableObject.CreateInstance<Consumable>(), 1));
        }

        Debug.Log("2. Simulating Spells & Dice...");
        for(int i = 0; i < 100; i++) rm.RecordSpellCast();
        for(int i = 0; i < 10; i++) rm.RecordMaxDiceRoll();

        Debug.Log("3. Simulating Combat (Stone Golems, Heretic, generic enemies, Boss)...");
        for(int i = 0; i < 5; i++) rm.RecordKill("Stone Golem", false);
        for(int i = 0; i < 10; i++) rm.RecordKill("Heretic", false);
        for(int i = 0; i < 185; i++) rm.RecordKill("Slime", false); // Total 200
        
        // Defeat final boss
        rm.RecordKill("Fallen Saint", true);

        // 5 Deaths
        for(int i = 0; i < 5; i++) rm.RecordDeath();
        
        Debug.Log("<color=green>--- RITE UNLOCK RESULTS ---</color>");
        Debug.Log($"Midas (1000 Gold): {rm.IsRiteUnlocked(RiteType.Midas.ToString())}");
        Debug.Log($"Gluttony (50 Items): {rm.IsRiteUnlocked(RiteType.Gluttony.ToString())}");
        Debug.Log($"Merlin (100 Spells): {rm.IsRiteUnlocked(RiteType.Merlin.ToString())}");
        Debug.Log($"Palamedes (10 Nat20s): {rm.IsRiteUnlocked(RiteType.Palamedes.ToString())}");
        Debug.Log($"Colossus (5 Golems): {rm.IsRiteUnlocked(RiteType.Colossus.ToString())}");
        Debug.Log($"Chalice (10 Heretics): {rm.IsRiteUnlocked(RiteType.Chalice.ToString())}");
        Debug.Log($"Beast (200 Enemies): {rm.IsRiteUnlocked(RiteType.Beast.ToString())}");
        Debug.Log($"Lazarus (5 Deaths): {rm.IsRiteUnlocked(RiteType.Lazarus.ToString())}");
        Debug.Log($"Afterbirth (Fallen Saint): {rm.IsRiteUnlocked(RiteType.Afterbirth.ToString())}");

        Debug.Log("<color=cyan>--- TESTS COMPLETED ---</color>");
    }
}
