﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Infrastructure.Identity.Services;

/// <summary>
/// Microsofts' System.<see cref="AuthenticateResult"/>
/// </summary>
public static class StringHelpers
{
    public static T ParseValueOrDefault<T>(string? stringValue, Func<string, T> parser, T defaultValue)
    {
        if (string.IsNullOrEmpty(stringValue))
        {
            return defaultValue;
        }

        return parser(stringValue);
    }
}