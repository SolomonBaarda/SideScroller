using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Presets
{
    // Game rules
    public bool DoMultiplayer;
    public bool DoEnemySpawning;
    public bool DoItemDrops;
    public bool DoGenerateItemsWithWorld;
    public bool DoSpawnWithRandomWeapons;

    // Map generation stuff
    public TerrainManager.Generation TerrainGenerationStyle;
    public Value TerrainWorldLengthIfNotEndless;

    // Objective stuff

    // Player controller stuff
    public Value PlayerSpeed;

    public Value GravityModifier;




    /// <summary>
    /// A list of all the default value enum types.
    /// </summary>
    public static List<Value> DefaultValues => new List<Value>(Enum.GetValues(typeof(Value)).Cast<Value>());
    /// <summary>
    /// A list of all possible default values as strings.
    /// </summary>
    public static List<string> DefaultValueStrings => new List<string>(Enumerable.Select(DefaultValues, x => x.ToString()));



    public Presets() : this(true, false, true, true, true, TerrainManager.Generation.Symmetrical_Limit, Value.Default, Value.Default, Value.Default) { }


    private Presets(bool DoMultiplayer, bool DoEnemySpawning, bool DoItemDrops, bool DoGenerateItemsWithWorld, bool DoSpawnWithRandomWeapons,
        TerrainManager.Generation TerrainGenerationStyle, Value TerrainWorldLengthIfNotEndless,
        Value GravityModifier, Value PlayerSpeed)
    {
        this.DoMultiplayer = DoMultiplayer;
        this.DoEnemySpawning = DoEnemySpawning;
        this.DoItemDrops = DoItemDrops;
        this.DoGenerateItemsWithWorld = DoGenerateItemsWithWorld;
        this.DoSpawnWithRandomWeapons = DoSpawnWithRandomWeapons;

        this.TerrainGenerationStyle = TerrainGenerationStyle;
        this.TerrainWorldLengthIfNotEndless = TerrainWorldLengthIfNotEndless;

        this.GravityModifier = GravityModifier;
        this.PlayerSpeed = PlayerSpeed;
    }


    public void SetPreset(Conversion ID, object o)
    {
        // Assign the correct variable
        switch (ID)
        {
            case Conversion.Multiplayer:
                DoMultiplayer = (bool)o;
                break;
            case Conversion.Item_Drops:
                DoItemDrops = (bool)o;
                break;
            case Conversion.Item_Spawns:
                DoGenerateItemsWithWorld = (bool)o;
                break;
            case Conversion.Random_Weapons:
                DoSpawnWithRandomWeapons = (bool)o;
                break;
            case Conversion.Map_Length:
                TerrainWorldLengthIfNotEndless = (Value)o;
                break;
            case Conversion.Gravity_Modifier:
                GravityModifier = (Value)o;
                break;
            case Conversion.Player_Speed:
                PlayerSpeed = (Value)o;
                break;
            default:
                Debug.LogError("Conversion ID undefined for type " + ID.ToString() + ".");
                break;
        }

    }


    public enum Conversion
    {
        Multiplayer,
        Item_Drops,
        Item_Spawns,
        Random_Weapons,
        Map_Length,
        Gravity_Modifier,
        Player_Speed,
    }


    public enum Value
    {
        Default,
        Less,
        More,
        Random,
    }


    public enum Type
    {
        Boolean,
        Value,
    }



    public static T CalculateVariableValue<T>(VariableValue<T> variable, Value chosen, System.Random random)
    {
        switch (chosen)
        {
            // Return basic values
            case Value.Default:
                return variable.Default;
            case Value.Less:
                return variable.Less;
            case Value.More:
                return variable.More;

            // Calculate the random value
            case Value.Random:
                try
                {
                    // First convert to double
                    double min = double.Parse(variable.Minimum.ToString());
                    double max = double.Parse(variable.Maximum.ToString());

                    // Then try and convert back to t
                    string s = (random.NextDouble() * (max - min) + min).ToString();

                    // Round it to the nearest whole number if we need to
                    if (variable.Default is int)
                    {
                        s = Mathf.RoundToInt(Utils.Parse<float>(s)).ToString();
                    }

                    return Utils.Parse<T>(s);
                }
                catch (Exception)
                {
                }
                break;

        }
        return variable.Default;
    }





    public class VariableValue<T>
    {
        public T Default;
        public T Less;
        public T More;
        public T Minimum;
        public T Maximum;

        public VariableValue(T defaultValue, T less, T more, T minimum, T maximum)
        {
            Default = defaultValue;
            Less = less;
            More = more;
            Minimum = minimum;
            Maximum = maximum;
        }
    }



    public new string ToString()
    {
        string s = "Preset values: " +
            "Multiplayer(" + DoMultiplayer + "), " +
            "Item Drops(" + DoItemDrops + "), " +
            "Item Spawn(" + DoGenerateItemsWithWorld + "), " +
            "Random Weapons(" + DoSpawnWithRandomWeapons + ")";

        return s;
    }
}
