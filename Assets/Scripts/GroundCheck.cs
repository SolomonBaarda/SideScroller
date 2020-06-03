using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GroundCheck : MonoBehaviour
{
    public const string GROUND_LAYER = "Ground";
    public const float DEFAULT_RADIUS = 0.2f;

    public static Collider2D[] GetGroundCollisions(Vector2 position, float collisionCheckRadius = DEFAULT_RADIUS)
    {
        // Get all collisions
        return Physics2D.OverlapCircleAll(position, collisionCheckRadius, LayerMask.GetMask(GROUND_LAYER));
    }

    public static bool IsOnGround(Vector2 position, float collisionCheckRadius = DEFAULT_RADIUS)
    {
        return GetGroundCollisions(position, collisionCheckRadius).Length != 0;
    }

}
