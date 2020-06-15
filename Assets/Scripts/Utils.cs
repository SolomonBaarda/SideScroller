using System;
using System.ComponentModel;

public static class Utils
{

    /// <summary>
    /// Parses a string to type T. Will fail if string is not identical to the desired format.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    public static T Parse<T>(string input)
    {
        // https://stackoverflow.com/questions/2961656/generic-tryparse

        try
        {
            // Get the converter for the type
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                // Convert to T
                // This will fail if string is not identical to type format
                return (T)converter.ConvertFromString(input);
            }
        }
        catch (Exception)
        {
        }

        throw new Exception("Failed to parse string " + input + " to " + typeof(T));
    }
}
