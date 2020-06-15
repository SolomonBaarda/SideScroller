using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Presets
{
    // Game rules
    public bool DoSinglePlayer;
    public bool DoEnemySpawning;
    public bool DoItemDrops;

    // Map generation stuff
    public TerrainManager.Generation terrain_generation;
    public int terrain_limit_not_endless;

    // Objective stuff

    // Player controller stuff
    public Value player_gravity;
    public Value player_speed;

    public Presets() : this(false, false, true, TerrainManager.Generation.Symmetrical_Limit, TerrainManager.DEAULT_WORLD_LENGTH_NOT_ENDLESS,
        Value.Default, Value.Default)
    {
    }

    public Presets(bool DoSinglePlayer, bool DoEnemySpawning, bool DoItemDrops,
        TerrainManager.Generation terrain_generation, int terrain_limit_not_endless,
        Value player_gravity, Value player_speed)
    {
        this.DoSinglePlayer = DoSinglePlayer;
        this.DoEnemySpawning = DoEnemySpawning;
        this.DoItemDrops = DoItemDrops;

        this.terrain_generation = terrain_generation;
        this.terrain_limit_not_endless = terrain_limit_not_endless;

        this.player_gravity = player_gravity;
        this.player_speed = player_speed;
    }


    public void SetPreset(Conversion ID, Value newValue)
    {

    }


    public enum Conversion
    {
        Player_Gravity,
        Player_Speed,
    }


    public enum Value
    {
        Default,
        Less,
        More,
        Random,
    }
}
