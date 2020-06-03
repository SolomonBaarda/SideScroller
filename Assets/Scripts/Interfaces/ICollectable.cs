using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollectable
{

    void Collect(Player player);

    void Drop(Vector2 position, Vector2 velocity);


}
