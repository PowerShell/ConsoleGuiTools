// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.PowerShell.ConsoleGuiTools;

/// <summary>
///     Represents an element within a collection member result, providing indexed access to collection items.
/// </summary>
internal sealed class CachedMemberResultElement
{
    #region Fields

    /// <summary>
    ///     The index of this element within the collection.
    /// </summary>
    public int Index;

    /// <summary>
    ///     The value of this collection element.
    /// </summary>
    public object? Value;

    private readonly string _representation;

    #endregion

    #region Constructor

    /// <summary>
    ///     Initializes a new instance of the <see cref="CachedMemberResultElement" /> class with the specified value and index.
    /// </summary>
    /// <param name="value">The value of the collection element.</param>
    /// <param name="index">The zero-based index of this element within the collection.</param>
    public CachedMemberResultElement(object? value, int index)
    {
        Index = index;
        Value = value;

        try
        {
            _representation = Value?.ToString() ?? "Null";
        }
        catch (Exception)
        {
            Value = _representation = "Unavailable";
        }
    }

    #endregion

    #region Overrides

    /// <summary>
    ///     Returns a string representation of this collection element in the format "[index]: value".
    /// </summary>
    /// <returns>A formatted string showing the index and value.</returns>
    public override string ToString() => $"[{Index}]: {_representation}";

    #endregion
}
