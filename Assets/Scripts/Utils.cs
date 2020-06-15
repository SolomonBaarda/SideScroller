using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public static class Utils
{
    public static T Parse<T>(string input)
    {
        // https://stackoverflow.com/questions/2961656/generic-tryparse
        try
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                try
                {
                    return (T)converter.ConvertFromString(input);
                }
                catch (Exception)
                {
                    // If we get here, then it failed to parse for some reason

                    // It may be int, try to truncate first instead
                    return Parse<T>(((int)Parse<double>(input)).ToString());
                }

            }
        }
        catch (Exception)
        {
        }

        return default;
    }
}
