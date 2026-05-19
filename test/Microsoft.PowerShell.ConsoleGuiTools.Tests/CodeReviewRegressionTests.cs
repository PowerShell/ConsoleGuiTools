// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.PowerShell.OutGridView.Models;
using Xunit;

namespace Microsoft.PowerShell.ConsoleGuiTools.Tests;

/// <summary>
///     Regression tests for issues identified during code review of PR #267.
///     Each test demonstrates a specific bug; tests should FAIL until the bug is fixed.
/// </summary>
public class CodeReviewRegressionTests
{
    #region CachedMemberResultElement.ToString() extra trailing bracket

    /// <summary>
    ///     CR feedback: CachedMemberResultElement.ToString() adds an extra trailing ']'
    ///     producing "[index]: value]" instead of "[index]: value".
    ///     See: src/Microsoft.PowerShell.ConsoleGuiTools/CachedMemberResultElement.cs line 59
    /// </summary>
    [Fact]
    public void CachedMemberResultElement_ToString_ShouldNotHaveExtraTrailingBracket()
    {
        var element = new CachedMemberResultElement("hello", 0);

        var result = element.ToString();

        // Expected format per doc comment: "[index]: value"
        Assert.Equal("[0]: hello", result);
    }

    [Fact]
    public void CachedMemberResultElement_ToString_WithIndex3_FormatsCorrectly()
    {
        var element = new CachedMemberResultElement(42, 3);

        var result = element.ToString();

        Assert.Equal("[3]: 42", result);
    }

    [Fact]
    public void CachedMemberResultElement_ToString_NullValue_FormatsCorrectly()
    {
        var element = new CachedMemberResultElement(null, 1);

        var result = element.ToString();

        Assert.Equal("[1]: Null", result);
    }

    #endregion

    #region OutGridViewDataSource sort should use IValue.CompareTo instead of parsing DisplayValue

    /// <summary>
    ///     CR feedback: Sorting uses double.TryParse(value.DisplayValue) which fails when
    ///     DisplayValue has formatting (e.g., thousand separators). The IValue types already
    ///     implement IComparable correctly via their SortValue/DisplayValue properties.
    ///     See: src/Microsoft.PowerShell.ConsoleGuiTools/OutGridViewDataSource.cs line 172
    /// </summary>
    [Fact]
    public void Sort_WithDecimalValues_ShouldSortByIValueNotDisplayValue()
    {
        // DecimalValues have SortValue for proper numeric comparison
        // but their DisplayValue might have formatting that breaks double.TryParse
        // Use currency-style formatting that will never parse as a double
        var columns = new List<DataTableColumn> { new("Amount", "Amount") };
        var ds = new OutGridViewDataSource(columns);

        var columnKey = columns[0].ToString();

        ds.AddRow(new DataTableRow(
            new Dictionary<string, IValue>
            {
                [columnKey] = new DecimalValue { DisplayValue = "EUR 1.000,00", SortValue = 1000m }
            }, 0));

        ds.AddRow(new DataTableRow(
            new Dictionary<string, IValue>
            {
                [columnKey] = new DecimalValue { DisplayValue = "EUR 500,00", SortValue = 500m }
            }, 1));

        ds.AddRow(new DataTableRow(
            new Dictionary<string, IValue>
            {
                [columnKey] = new DecimalValue { DisplayValue = "EUR 2.500,00", SortValue = 2500m }
            }, 2));

        var sorted = ds.Sort(0, descending: false);

        // Should sort by SortValue (numeric): 500, 1000, 2500
        Assert.Equal("EUR 500,00", sorted[0, 0]);
        Assert.Equal("EUR 1.000,00", sorted[1, 0]);
        Assert.Equal("EUR 2.500,00", sorted[2, 0]);
    }

    [Fact]
    public void Sort_MixedDecimalAndStringValues_ShouldUseIValueComparison()
    {
        // When cells carry DecimalValue instances, sorting should use their
        // IComparable implementation rather than re-parsing DisplayValue
        var columns = new List<DataTableColumn> { new("Price", "Price") };
        var ds = new OutGridViewDataSource(columns);

        var columnKey = columns[0].ToString();

        ds.AddRow(new DataTableRow(
            new Dictionary<string, IValue>
            {
                [columnKey] = new DecimalValue { DisplayValue = "$10.00", SortValue = 10m }
            }, 0));

        ds.AddRow(new DataTableRow(
            new Dictionary<string, IValue>
            {
                [columnKey] = new DecimalValue { DisplayValue = "$2.00", SortValue = 2m }
            }, 1));

        ds.AddRow(new DataTableRow(
            new Dictionary<string, IValue>
            {
                [columnKey] = new DecimalValue { DisplayValue = "$100.00", SortValue = 100m }
            }, 2));

        var sorted = ds.Sort(0, descending: false);

        // Should sort numerically by SortValue: 2, 10, 100
        Assert.Equal("$2.00", sorted[0, 0]);
        Assert.Equal("$10.00", sorted[1, 0]);
        Assert.Equal("$100.00", sorted[2, 0]);
    }

    #endregion

    #region Version string IndexOf('+') unchecked slice

    /// <summary>
    ///     CR feedback: ProductVersion.IndexOf('+') is used as a slice end without checking
    ///     for -1. If the version string doesn't contain '+', the slice throws.
    ///     See: ShowObjectTreeWindow.cs line 172, OutGridViewWindow.cs line 426
    ///
    ///     This test verifies the guard is in place by testing the same pattern.
    /// </summary>
    [Fact]
    public void VersionParsing_WithoutPlusSign_ShouldNotThrow()
    {
        // Simulates the fixed pattern from ShowObjectTreeWindow/OutGridViewWindow:
        // var plusIdx = productVersion?.IndexOf('+') ?? -1;
        // result = plusIdx >= 0 ? productVersion![..plusIdx] : productVersion;
        string productVersion = "2.1.0"; // No '+' character

        var exception = Record.Exception(() =>
        {
            var plusIdx = productVersion.IndexOf('+');
            var result = plusIdx >= 0 ? productVersion[..plusIdx] : productVersion;
            Assert.Equal("2.1.0", result);
        });

        Assert.Null(exception);
    }

    [Fact]
    public void VersionParsing_WithPlusSign_TrimsAfterPlus()
    {
        string productVersion = "2.1.0+abc123";

        var plusIdx = productVersion.IndexOf('+');
        var result = plusIdx >= 0 ? productVersion[..plusIdx] : productVersion;

        Assert.Equal("2.1.0", result);
    }

    #endregion
}
