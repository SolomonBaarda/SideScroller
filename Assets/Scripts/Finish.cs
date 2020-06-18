using UnityEngine;

public class Finish : MonoBehaviour
{
    public Bounds Bounds { get; private set; }
    public Payload.Direction WinningDirection { get; private set; }

    public void CreateFinish(Bounds bounds, Payload.Direction winner)
    {
        Bounds = bounds;
        WinningDirection = winner;
    }




    private void FixedUpdate()
    {
        CheckCollisions();
    }


    private void CheckCollisions()
    {
        Collider2D[] collisions = Physics2D.OverlapBoxAll(Bounds.center, Bounds.size, 0, LayerMask.GetMask(ItemManager.ITEM_LAYER));

        // Check each collision
        foreach (Collider2D c in collisions)
        {
            // The payload has collided
            if(WorldItem.ExtendsClass<Payload>(c.gameObject))
            {
                // End the game
                GameManager.OnGameEnd.Invoke(WinningDirection);
            }
        }
    }



    private void OnDrawGizmos()
    {
        if (Bounds != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawLine(Bounds.min, new Vector2(Bounds.max.x, Bounds.min.y));
            Gizmos.DrawLine(Bounds.min, new Vector2(Bounds.min.x, Bounds.max.y));
            Gizmos.DrawLine(Bounds.max, new Vector2(Bounds.max.x, Bounds.min.y));
            Gizmos.DrawLine(Bounds.max, new Vector2(Bounds.min.x, Bounds.max.y));
        }
    }
}
