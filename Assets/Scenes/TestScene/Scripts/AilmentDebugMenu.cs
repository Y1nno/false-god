using UnityEngine;

public class AilmentDebugMenu : MonoBehaviour
{
    private CombatManager _cbm;

    private void EnsureManager()
    {
        if (_cbm == null)
        {
            _cbm = RunManager.Instance.GetService<CombatManager>();
        }
    }

    [ContextMenu("Apply All Ailments")]
    public void ApplyAll()
    {
        EnsureManager();
        if (_cbm != null && _cbm.CurrentBattle != null && _cbm.CurrentBattle.Pcm != null)
        {
            _cbm.CurrentBattle.Pcm.ApplyAilment(AilmentType.Burn, 3);
            _cbm.CurrentBattle.Pcm.ApplyAilment(AilmentType.Poison, 3);
            _cbm.CurrentBattle.Pcm.ApplyAilment(AilmentType.Frozen, 3);
            _cbm.CurrentBattle.Pcm.ApplyAilment(AilmentType.Bleed, 3);
            RefreshUI();
        }
    }

    [ContextMenu("Remove All Ailments")]
    public void RemoveAll()
    {
        EnsureManager();
        if (_cbm != null && _cbm.CurrentBattle != null && _cbm.CurrentBattle.Pcm != null)
        {
            _cbm.CurrentBattle.Pcm.RemoveAilment(AilmentType.Burn);
            _cbm.CurrentBattle.Pcm.RemoveAilment(AilmentType.Poison);
            _cbm.CurrentBattle.Pcm.RemoveAilment(AilmentType.Frozen);
            _cbm.CurrentBattle.Pcm.RemoveAilment(AilmentType.Bleed);
            RefreshUI();
        }
    }

    // --- Specific Ailments ---

    [ContextMenu("Apply/Burn")]
    public void ApplyBurn() { ApplySpecific(AilmentType.Burn, 3); }
    [ContextMenu("Remove/Burn")]
    public void RemoveBurn() { RemoveSpecific(AilmentType.Burn); }

    [ContextMenu("Apply/Poison")]
    public void ApplyPoison() { ApplySpecific(AilmentType.Poison, 3); }
    [ContextMenu("Remove/Poison")]
    public void RemovePoison() { RemoveSpecific(AilmentType.Poison); }

    [ContextMenu("Apply/Frozen")]
    public void ApplyFrozen() { ApplySpecific(AilmentType.Frozen, 3); }
    [ContextMenu("Remove/Frozen")]
    public void RemoveFrozen() { RemoveSpecific(AilmentType.Frozen); }

    [ContextMenu("Apply/Bleed")]
    public void ApplyBleed() { ApplySpecific(AilmentType.Bleed, 3); }
    [ContextMenu("Remove/Bleed")]
    public void RemoveBleed() { RemoveSpecific(AilmentType.Bleed); }

    private void ApplySpecific(AilmentType type, int duration)
    {
        EnsureManager();
        if (_cbm != null && _cbm.CurrentBattle != null && _cbm.CurrentBattle.Pcm != null)
        {
            _cbm.CurrentBattle.Pcm.ApplyAilment(type, duration);
            RefreshUI();
        }
        else
        {
            Debug.LogWarning("You must be in an active battle to apply an ailment.");
        }
    }

    private void RemoveSpecific(AilmentType type)
    {
        EnsureManager();
        if (_cbm != null && _cbm.CurrentBattle != null && _cbm.CurrentBattle.Pcm != null)
        {
            _cbm.CurrentBattle.Pcm.RemoveAilment(type);
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        InputInterface inputUI = GameObject.FindAnyObjectByType<InputInterface>();
        if (inputUI != null) inputUI.RefreshUI();
    }
}
