// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

public static class EnumExtensions
{
    public static string GetDescriptionFromEnumValue(this Enum value)
    {
        return value
                 .GetType()
                 .GetMember(value.ToString())
                 .FirstOrDefault()
                 ?.GetCustomAttribute<DescriptionAttribute>()
                 ?.Description
             ?? value.ToString();
    }


}
