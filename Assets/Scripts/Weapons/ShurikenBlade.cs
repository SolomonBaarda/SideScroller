using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShurikenBlade : MonoBehaviour
{
    public Collider2D blade;

    public const float speed = 2f;
    private bool hasCollided;

    public List<GameObject> AreaOfAttack => PlayerInteraction.InAreaOfAttack(blade, transform.parent.parent.gameObject);


    public IEnumerator Throw(Vector2 attackerPosition, Vector2 attackerVelocity, Player.Facing direction)
    {
        transform.position = attackerPosition;
        hasCollided = false;

        float distance = 1;
        if (direction == Player.Facing.Left)
        {
            distance = -speed;
        }
        else if (direction == Player.Facing.Right)
        {
            distance = speed;
        }

        while (!hasCollided)
        {
            // Move
            Vector2 pos = transform.position;
            pos.x += distance * Time.deltaTime;

            transform.position = pos;

            // Wait for next frame
            yield return null;
        }

        Destroy(gameObject);
    }


    public void SetHasCollided()
    {
        hasCollided = true;
    }

}
