using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UIElements;

public class Presets
{
    // Game rules
    public bool DoSinglePlayer;
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



    public Presets() : this(false, false, true, true, true, TerrainManager.Generation.Symmetrical_Limit, Value.Default, Value.Default, Value.Default) { }


    private Presets(bool DoSinglePlayer, bool DoEnemySpawning, bool DoItemDrops, bool DoGenerateItemsWithWorld, bool DoSpawnWithRandomWeapons,
        TerrainManager.Generation TerrainGenerationStyle, Value TerrainWorldLengthIfNotEndless,
        Value GravityModifier, Value PlayerSpeed)
    {
        this.DoSinglePlayer = DoSinglePlayer;
        this.DoEnemySpawning = DoEnemySpawning;
        this.DoItemDrops = DoItemDrops;
        this.DoGenerateItemsWithWorld = DoGenerateItemsWithWorld;
        this.DoSpawnWithRandomWeapons = DoSpawnWithRandomWeapons;

        this.TerrainGenerationStyle = TerrainGenerationStyle;
        this.TerrainWorldLengthIfNotEndless = TerrainWorldLengthIfNotEndless;

        this.GravityModifier = GravityModifier;
        this.PlayerSpeed = PlayerSpeed;
    }


    public void SetPreset(Conversion ID, object newValue)
    {
        // Assign the correct variable
        switch (ID)
        {
            case Conversion.Map_Length:
                TerrainWorldLengthIfNotEndless = (Value)newValue;
                break;
            case Conversion.Gravity_Modifier:
                GravityModifier = (Value)newValue;
                break;
            case Conversion.Player_Speed:
                PlayerSpeed = (Value)newValue;
                break;
            case Conversion.Single_Player:
                DoSinglePlayer = (bool)newValue;
                break;
            default:
                Debug.LogError("Conversion ID undefined for type " + ID.ToString() + ".");
                break;
        }

    }


    public enum Conversion
    {
        Map_Length,
        Gravity_Modifier,
        Player_Speed,
        Single_Player,
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
                return variable.Minimum;
            case Value.More:
                return variable.Maximum;

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
        public T Minimum;
        public T Maximum;

        public VariableValue(T defaultValue, T minimum, T maximum)
        {
            Default = defaultValue;
            Minimum = minimum;
            Maximum = maximum;
        }
    }
}
