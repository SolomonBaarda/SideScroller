using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff : ScriptableObject
{
    protected BuffName buffName;

    // Movement buffs
    protected float speed_bonus = 0;
    protected float jump_power_bonus = 0;
    protected int double_jump_bonus = 0;

    // Damage buffs
    protected float damage_bonus = 0;
    protected float projectile_speed_bonus = 0;
    protected float splash_damage_area_bonus = 0;

    // Health buffs
    protected int max_health_bonus = 0;
    protected float damage_resistance_bonus = 0;

    // Inventory buffs
    protected int weapon_inventory_size_bonus = 0;


    public void CombineBuffs(Buff b)
    {
        // Combine all field totals 
    }

    public enum BuffName
    {

    }

}
