using UnityEngine;

public class LazarusRite : Rite, IObserver
{
    private PlayerManager _playerManager;

    public override string Description => "Lazarus: Prevents death once and restores 50% HP.";

    public LazarusRite() : base("Lazarus", 10) // ID: Lazarus, Cost: 10
    {
    }

    public override void OnEquip(PlayerManager player)
    {
        _playerManager = player;
        _playerManager.AttachObserver(this);
        TextOutputter.Instance.OutputText("Lazarus watches over you...");
    }

    public override void OnUnequip(PlayerManager player)
    {
        if (_playerManager != null)
        {
            _playerManager.DetachObserver(this);
        }
    }

    public void OnNotify(object subject, EventType eventType)
    {
        if (eventType == EventType.PlayerAboutToDie)
        {
            PreventDeath();
        }
    }

    private void PreventDeath()
    {
        int maxHealth = _playerManager.Health.MaxValue;
        int targetHealth = Mathf.RoundToInt(maxHealth * 0.5f);
        
        // Ensure we actually heal above 0 (e.g. if MaxHP is tiny)
        if (targetHealth <= 0) targetHealth = 1;

        _playerManager.Health.SetCurrent(targetHealth);
        
        TextOutputter.Instance.OutputText("Lazarus Resurrected You! (HP Restored to 50%)");
        
        // This Rite consumes itself
        RunManager.Instance.GetService<RiteManager>().UnequipRite(this);
    }
}
