using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpellManager : GameModule
{
    private List<SpellSO> _learnedSpells = new List<SpellSO>();
    public const int MaxSpells = 6;

    public List<SpellSO> LearnedSpells => _learnedSpells;

    public void RestoreState(List<string> spellIDs)
    {
        _learnedSpells.Clear();
        foreach (var id in spellIDs)
        {
            // Try different name patterns (Spaced vs Non-Spaced)
            SpellSO spell = Resources.Load<SpellSO>($"Spells/{id}");
            if (spell == null) spell = Resources.Load<SpellSO>($"Spells/{id.Replace(" ", "")}");
            
            if (spell != null) _learnedSpells.Add(spell);
        }
        Notify(EventType.EquipmentChanged);
    }

    public override void AttachDefaultObservers() { }

    public bool LearnSpell(SpellSO spell)
    {
        if (_learnedSpells.Count >= MaxSpells)
        {
            TextOutputter.Instance.OutputText("You already know too many spells! Forget one first.");
            return false;
        }

        if (_learnedSpells.Any(s => s.actionName == spell.actionName))
        {
            TextOutputter.Instance.OutputText($"You already know {spell.actionName}!");
            return false;
        }

        _learnedSpells.Add(spell);
        TextOutputter.Instance.OutputText($"You have learned {spell.actionName}!");
        Notify(EventType.EquipmentChanged); // Hack to refresh UI
        return true;
    }

    public void ForgetSpell(SpellSO spell)
    {
        if (_learnedSpells.Remove(spell))
        {
            TextOutputter.Instance.OutputText($"You have forgotten {spell.actionName}.");
            Notify(EventType.EquipmentChanged);
        }
    }
}
