using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnumUtil
{
    public static string[] FromEnumToStringArray(Type enumType)
    {
        return Enum.GetNames(enumType);
    }

    public static Array FromEnumToArray(Type enumType)
    {
        return Enum.GetValues(enumType);
    }

    public static T ToEnum<T>(string value, T defaultValue) where T : struct
    {
        if(string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return Enum.TryParse<T>(value, true, out T result) ? result : defaultValue;
    }
}
