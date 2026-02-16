using UnityEngine;

public class RiteDebugMenu : MonoBehaviour
{
    private RiteManager _rm;

    private void EnsureManager()
    {
        if (_rm == null)
        {
            _rm = RunManager.Instance.GetService<RiteManager>();
        }
    }

    [ContextMenu("Unlock All Rites")]
    public void UnlockAll()
    {
        EnsureManager();
        string[] allRites = new string[] 
        { 
            "Candle", "Colossus", "Ouroboros", "Judgement", "Lazarus", 
            "Midas", "Chalice", "Palamedes", "Gluttony", "Juggernaut", 
            "Berserk", "Empress", "Beast", "Merlin", "Faithless" 
        };

        foreach (var id in allRites)
        {
            _rm.UnlockRite(id);
        }
    }

    [ContextMenu("Lock All Rites")]
    public void LockAll()
    {
        EnsureManager();
         string[] allRites = new string[] 
        { 
            "Candle", "Colossus", "Ouroboros", "Judgement", "Lazarus", 
            "Midas", "Chalice", "Palamedes", "Gluttony", "Juggernaut", 
            "Berserk", "Empress", "Beast", "Merlin", "Faithless" 
        };

        foreach (var id in allRites)
        {
            _rm.LockRite(id);
        }
    }

    // --- Specific Rites ---

    [ContextMenu("Unlock/Candle")]
    public void UnlockCandle() { EnsureManager(); _rm.UnlockRite("Candle"); }
    [ContextMenu("Lock/Candle")]
    public void LockCandle() { EnsureManager(); _rm.LockRite("Candle"); }

    [ContextMenu("Unlock/Colossus")]
    public void UnlockColossus() { EnsureManager(); _rm.UnlockRite("Colossus"); }
    [ContextMenu("Lock/Colossus")]
    public void LockColossus() { EnsureManager(); _rm.LockRite("Colossus"); }

    [ContextMenu("Unlock/Ouroboros")]
    public void UnlockOuroboros() { EnsureManager(); _rm.UnlockRite("Ouroboros"); }
    [ContextMenu("Lock/Ouroboros")]
    public void LockOuroboros() { EnsureManager(); _rm.LockRite("Ouroboros"); }

    [ContextMenu("Unlock/Judgement")]
    public void UnlockJudgement() { EnsureManager(); _rm.UnlockRite("Judgement"); }
    [ContextMenu("Lock/Judgement")]
    public void LockJudgement() { EnsureManager(); _rm.LockRite("Judgement"); }

    [ContextMenu("Unlock/Lazarus")]
    public void UnlockLazarus() { EnsureManager(); _rm.UnlockRite("Lazarus"); }
    [ContextMenu("Lock/Lazarus")]
    public void LockLazarus() { EnsureManager(); _rm.LockRite("Lazarus"); }

    [ContextMenu("Unlock/Midas")]
    public void UnlockMidas() { EnsureManager(); _rm.UnlockRite("Midas"); }
    [ContextMenu("Lock/Midas")]
    public void LockMidas() { EnsureManager(); _rm.LockRite("Midas"); }

    [ContextMenu("Unlock/Chalice")]
    public void UnlockChalice() { EnsureManager(); _rm.UnlockRite("Chalice"); }
    [ContextMenu("Lock/Chalice")]
    public void LockChalice() { EnsureManager(); _rm.LockRite("Chalice"); }

    [ContextMenu("Unlock/Palamedes")]
    public void UnlockPalamedes() { EnsureManager(); _rm.UnlockRite("Palamedes"); }
    [ContextMenu("Lock/Palamedes")]
    public void LockPalamedes() { EnsureManager(); _rm.LockRite("Palamedes"); }

    [ContextMenu("Unlock/Gluttony")]
    public void UnlockGluttony() { EnsureManager(); _rm.UnlockRite("Gluttony"); }
    [ContextMenu("Lock/Gluttony")]
    public void LockGluttony() { EnsureManager(); _rm.LockRite("Gluttony"); }

    [ContextMenu("Unlock/Juggernaut")]
    public void UnlockJuggernaut() { EnsureManager(); _rm.UnlockRite("Juggernaut"); }
    [ContextMenu("Lock/Juggernaut")]
    public void LockJuggernaut() { EnsureManager(); _rm.LockRite("Juggernaut"); }

    [ContextMenu("Unlock/Berserk")]
    public void UnlockBerserk() { EnsureManager(); _rm.UnlockRite("Berserk"); }
    [ContextMenu("Lock/Berserk")]
    public void LockBerserk() { EnsureManager(); _rm.LockRite("Berserk"); }

    [ContextMenu("Unlock/Empress")]
    public void UnlockEmpress() { EnsureManager(); _rm.UnlockRite("Empress"); }
    [ContextMenu("Lock/Empress")]
    public void LockEmpress() { EnsureManager(); _rm.LockRite("Empress"); }

    [ContextMenu("Unlock/Beast")]
    public void UnlockBeast() { EnsureManager(); _rm.UnlockRite("Beast"); }
    [ContextMenu("Lock/Beast")]
    public void LockBeast() { EnsureManager(); _rm.LockRite("Beast"); }

    [ContextMenu("Unlock/Merlin")]
    public void UnlockMerlin() { EnsureManager(); _rm.UnlockRite("Merlin"); }
    [ContextMenu("Lock/Merlin")]
    public void LockMerlin() { EnsureManager(); _rm.LockRite("Merlin"); }

    [ContextMenu("Unlock/Faithless")]
    public void UnlockFaithless() { EnsureManager(); _rm.UnlockRite("Faithless"); }
    [ContextMenu("Lock/Faithless")]
    public void LockFaithless() { EnsureManager(); _rm.LockRite("Faithless"); }
}
