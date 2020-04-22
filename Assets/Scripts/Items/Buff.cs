using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu]
public class Buff : Item
{
    [SerializeField] private Name buffName;

    // Movement buffs
    [SerializeField] [Range(0, 1)] private float speed_bonus_multiplier;
    [SerializeField] [Range(0, 1)] private float jump_power_bonus_multiplier;
    [SerializeField] [Range(0, 1)] private int double_jump_bonus;

    // Damage buffs
    [SerializeField] [Range(0, 1)] private float damage_bonus_multiplier;
    [SerializeField] [Range(0, 1)] private float projectile_speed_bonus_multiplier;
    [SerializeField] [Range(0, 1)] private float splash_damage_area_bonus_multiplier;

    // Health buffs
    [SerializeField] [Range(0, 1)] private float max_health_bonus_multiplier;
    [SerializeField] [Range(0, 1)] private float damage_resistance_bonus_multiplier;

    // Inventory buffs
    [SerializeField] [Range(0, 1)] private int weapon_inventory_size_bonus = 0;


    public void CombineBuffs(Buff b)
    {
        speed_bonus_multiplier += b.speed_bonus_multiplier;
        jump_power_bonus_multiplier += b.jump_power_bonus_multiplier;
        double_jump_bonus += b.double_jump_bonus;

        damage_bonus_multiplier += b.damage_bonus_multiplier;
        projectile_speed_bonus_multiplier += b.projectile_speed_bonus_multiplier;
        splash_damage_area_bonus_multiplier += b.splash_damage_area_bonus_multiplier;

        max_health_bonus_multiplier += b.max_health_bonus_multiplier;
        damage_resistance_bonus_multiplier += b.damage_resistance_bonus_multiplier;

        weapon_inventory_size_bonus += b.weapon_inventory_size_bonus;
    }



    public enum Name
    {
        TOTAL_BUFFS_COMBINED,
        SpeedBoost,
        Jump_Power,
        DoubleJump
    }

}
