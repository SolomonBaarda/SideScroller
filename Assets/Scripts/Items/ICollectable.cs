using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollectable
{

    void Collect(PlayerInventory inventory);

    void Drop(Vector2 position, Vector2 velocity);


}
