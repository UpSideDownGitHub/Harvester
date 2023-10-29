using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerHolding : NetworkBehaviour
{
    [Header("Holding")]
    public bool isOwner;
    public SpriteRenderer spriteRenderer;
    public ItemData items;
    [SyncVar(OnChange = "SetIcon")] int itemID;

    [Header("Health")]
    [SyncVar(OnChange = "HealthChanged")] public bool health;
    public Player player;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            isOwner = true;
            gameObject.GetComponent<PlayerHolding>().enabled = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetHolding(int itemID)
    {
        this.itemID = itemID;
    }

    public void SetIcon(int oldValue, int newValue, bool asServer)
    {
        if (asServer)
            return;

        spriteRenderer.sprite = items.items[newValue].icon;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetHealthChange(bool value)
    {
        this.health = value;
    }

    public void HealthChanged(bool oldValue, bool newValue, bool asServer)
    {
        if (asServer)
            return;

        player.DecreaseHealth();
    }
}
