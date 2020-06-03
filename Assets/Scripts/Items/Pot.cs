using UnityEngine;

public class Pot : WorldItem, IInteractable, ILootable, ILoot, ICanBeAttacked, ICanBeHeld
{
    public LootTable table;

    [SerializeField]
    private int inventory_size = 1;

    private bool hasContents = true;
    private bool isBeingHeld = false;

    private Rigidbody2D rigid;

    public Transform groundPosition;

    public Transform GroundPosition { get { return groundPosition; } }

    new private void Awake()
    {
        base.Awake();

        rigid = GetComponent<Rigidbody2D>();
    }


    public void Interact(Player player)
    {
        if (player.Inventory.PickUp(gameObject))
        {
            Hold(player, player.Head.localPosition);
        }
    }


    public LootTable GetLootTable()
    {
        return table;
    }


    public int GetTotalItemsToBeLooted()
    {
        return inventory_size;
    }

    public bool IsLootable()
    {
        return hasContents && !isBeingHeld;
    }

    public void Loot()
    {
        hasContents = false;
    }

    public void WasAttacked(Vector2 attackerPosition, Vector2 attackerVelocity)
    {
        Break();
    }

    private void Break()
    {
        InteractionManager.OnInteractWithItem(gameObject);

        // Break the pot
        Animator a = GetComponent<Animator>();
        a.SetTrigger("Break");
    }

    private void CheckIfBreakWhenThrown()
    {
        // Check if we have just collided with the ground
        float radius = Mathf.Abs(GroundPosition.localPosition.y);
        if (GroundCheck.IsOnGround(transform.position, radius))
        {
            Break();
        }
    }

    public void Hold(Player player, Vector2 localPosition)
    {
        isBeingHeld = true;

        transform.parent = player.transform;
        transform.localPosition = localPosition;

        rigid.velocity = Vector2.zero;
        rigid.isKinematic = true;

        trigger.enabled = false;
    }

    public void Drop(Vector2 position, Vector2 velocity)
    {
        isBeingHeld = false;

        transform.parent = null;
        transform.position = position;

        rigid.isKinematic = false;
        rigid.velocity = velocity;

        trigger.enabled = true;

        InvokeRepeating("CheckIfBreakWhenThrown", 1, 0.1f);
    }

    public void SetLocalPosition(Vector2 local)
    {
        local.y += Mathf.Abs(GroundPosition.localPosition.y);
        transform.localPosition = local;
    }
}
