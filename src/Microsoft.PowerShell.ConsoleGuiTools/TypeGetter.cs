using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using Microsoft.PowerShell.OutGridView.Models;

namespace Microsoft.PowerShell.ConsoleGuiTools;

/// <summary>
///     Provides methods to retrieve type information and convert PowerShell objects into data table structures for display
///     in the grid view using PowerShell's native formatting infrastructure.
/// </summary>
public class TypeGetter
{
    /// <summary>
    ///     Regex pattern to match ANSI escape sequences.
    /// </summary>
    private static readonly Regex AnsiEscapeRegex = new(@"\x1b\[[0-9;]*m", RegexOptions.Compiled);

    /// <summary>
    ///     Types that are considered primitives to PowerShell but not to C#.
    /// </summary>
    private static readonly List<string> ADDITIONAL_PRIMITIVE_TYPES =
    [
        "System.String",
        "System.Decimal",
        "System.IntPtr",
        "System.Security.SecureString",
        "System.Numerics.BigInteger"
    ];

    private readonly Dictionary<string, FormatViewDefinition?> _formatCache = new();

    /// <summary>
    ///     Strips ANSI escape sequences from a string.
    /// </summary>
    /// <param name="value">The string potentially containing ANSI codes.</param>
    /// <returns>The string with ANSI codes removed.</returns>
    private static string StripAnsiCodes(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return AnsiEscapeRegex.Replace(value, string.Empty);
    }

    /// <summary>
    ///     Gets the format view definition for the specified type name, using a cache to avoid redundant lookups.
    /// </summary>
    /// <param name="typeName">The full type name to get the format view definition for.</param>
    /// <returns>The format view definition if found; otherwise, <see langword="null" />.</returns>
    private FormatViewDefinition? GetFormatViewDefinitionForType(string typeName)
    {
        if (_formatCache.TryGetValue(typeName, out var cached)) return cached;

        try
        {
            // Use the current runspace to access format data from loaded modules
            // This is critical for CIM instances (like Get-NetAdapter) which have format data
            // defined in their respective modules' .ps1xml files
            using var ps = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace);
            ps.AddCommand("Get-FormatData").AddParameter("TypeName", typeName);

            var results = ps.Invoke();

            FormatViewDefinition? result = null;
            if (results.Count > 0)
            {
                var extendedTypeDefinition = results[0].BaseObject as ExtendedTypeDefinition;
                result = extendedTypeDefinition?.FormatViewDefinition.FirstOrDefault(v => v.Control is TableControl);
            }

            _formatCache[typeName] = result;
            return result;
        }
        catch
        {
            // If we can't get format data (e.g., not running in a runspace context),
            // cache null and continue with fallback logic
            _formatCache[typeName] = null;
            return null;
        }
    }

    /// <summary>
    ///     Gets the format view definition for the specified PowerShell object.
    /// </summary>
    /// <param name="obj">The PowerShell object to get the format view definition for.</param>
    /// <returns>The format view definition if found; otherwise, <see langword="null" />.</returns>
    private FormatViewDefinition? GetFormatViewDefinitionForObject(PSObject obj)
    {
        // PSObject has a TypeNames collection that includes PowerShell-specific type names
        // These are what the format system uses, not the .NET type name
        // For example, Get-NetAdapter returns objects with TypeName like "Microsoft.Management.Infrastructure.CimInstance#ROOT/StandardCimv2/MSFT_NetAdapter"

        foreach (var typeName in obj.TypeNames)
        {
            if (_formatCache.TryGetValue(typeName, out var cached))
                return cached;

            var fvd = GetFormatViewDefinitionForType(typeName);
            if (fvd != null) return fvd;
        }

        // Fallback to base object type name
        var baseTypeName = obj.BaseObject.GetType().FullName;
        if (baseTypeName is not null)
        {
            if (_formatCache.TryGetValue(baseTypeName, out var cached))
                return cached;

            return GetFormatViewDefinitionForType(baseTypeName);
        }

        return null;
    }

    /// <summary>
    ///     Gets the default display property set (TableContent) for a PowerShell object.
    ///     This represents the subset of properties that PowerShell displays by default.
    /// </summary>
    /// <param name="obj">The PowerShell object to examine.</param>
    /// <returns>A list of property names to display, or null if no default display set is defined.</returns>
    private static List<string>? GetDefaultDisplayPropertySet(PSObject obj)
    {
        try
        {
            // For CIM instances and other objects, PowerShell adds PSStandardMembers
            // through the Extended Type System (ETS), not always as instance members

            // First check instance members (for objects with runtime-added members)
            var standardMembers = obj.Members["PSStandardMembers"]?.Value as PSMemberSet;
            var defaultDisplayProperty = standardMembers?.Members["DefaultDisplayPropertySet"]?.Value as PSPropertySet;

            if (defaultDisplayProperty?.ReferencedPropertyNames is { Count: > 0 })
                return defaultDisplayProperty.ReferencedPropertyNames.ToList();

            // Second, check PSObject.Properties for DefaultDisplayPropertySet
            // Some objects have this defined through type adapters
            var psStandardMembers = obj.Properties["PSStandardMembers"];
            if (psStandardMembers?.Value is PSMemberSet memberSet)
            {
                var displayPropSet = memberSet.Members["DefaultDisplayPropertySet"] as PSPropertySet;
                if (displayPropSet?.ReferencedPropertyNames is { Count: > 0 })
                    return displayPropSet.ReferencedPropertyNames.ToList();
            }
        }
        catch
        {
            // If we can't get the default display property set, return null
        }

        return null;
    }

    /// <summary>
    ///     Retrieves the column definitions for the specified PowerShell objects based on their format view definitions or
    ///     properties.
    /// </summary>
    /// <param name="psObjects">The list of PowerShell objects to analyze.</param>
    /// <returns>A distinct list of data table columns.</returns>
    private List<DataTableColumn> GetDataColumnsForObject(List<PSObject> psObjects)
    {
        if (psObjects.Count == 0) return [];
        return GetDataColumnsForObject(psObjects[0]);
    }

    /// <summary>
    ///     Determines the data columns from a single PSObject (using format view definitions, DefaultDisplayPropertySet, or
    ///     all properties).
    /// </summary>
    internal List<DataTableColumn> GetDataColumnsForObject(PSObject firstObject)
    {
        var dataColumns = new List<DataTableColumn>();

        List<string> labels;
        List<string> propertyAccessors;

        // Priority order:
        // 1. Format view definition (from .ps1xml files) - what cmdlets like Get-NetAdapter use
        // 2. DefaultDisplayPropertySet (TableContent) - used by custom objects
        // 3. All properties (fallback)

        var fvd = GetFormatViewDefinitionForObject(firstObject);
        var defaultDisplayProps = GetDefaultDisplayPropertySet(firstObject);

        if (fvd?.Control is TableControl tableControl)
        {
            // Use the table format definition (THIS IS WHAT GET-NETADAPTER USES)
            var definedColumnLabels = tableControl.Headers.Select(h => h.Label).ToList();
            var displayEntries = tableControl.Rows[0].Columns.Select(c => c.DisplayEntry).ToArray();
            var propertyLabels = displayEntries.Select(de => de.Value).ToList();

            // Use the TypeDefinition Label if available otherwise just use the property name as a label
            labels = definedColumnLabels.Zip(propertyLabels, (definedLabel, propLabel) =>
            {
                if (string.IsNullOrEmpty(definedLabel)) return propLabel;
                return definedLabel;
            }).ToList();

            propertyAccessors = displayEntries.Select(de =>
                    de.ValueType == DisplayEntryValueType.Property
                        ? $"$_.\"{de.Value}\""
                        : de.Value // ScriptBlock
            ).ToList();
        }
        else if (defaultDisplayProps is { Count: > 0 })
        {
            // Use the DefaultDisplayPropertySet (for custom objects)
            labels = defaultDisplayProps;
            propertyAccessors = defaultDisplayProps.Select(p => $"$_.\"{p}\"").ToList();
        }
        else if (PsObjectIsPrimitive(firstObject))
        {
            // Handle primitive types
            labels = [firstObject.BaseObject.GetType().Name];
            propertyAccessors = ["$_"];
        }
        else
        {
            // Fallback to all properties
            labels = firstObject.Properties.Select(p => p.Name).ToList();
            propertyAccessors = firstObject.Properties.Select(p => $"$_.\"{p.Name}\"").ToList();
        }

        for (var i = 0; i < labels.Count; i++)
        {
            var column = new DataTableColumn(labels[i], propertyAccessors[i]);
            dataColumns.Add(column);
        }

        return dataColumns.Distinct().ToList();
    }

    /// <summary>
    ///     Determines whether the specified PowerShell object represents a primitive type.
    /// </summary>
    private static bool PsObjectIsPrimitive(PSObject ps)
    {
        var psBaseType = ps.BaseObject.GetType();
        return psBaseType.IsPrimitive || psBaseType.IsEnum ||
               ADDITIONAL_PRIMITIVE_TYPES.Contains(psBaseType.FullName!);
    }

    /// <summary>
    ///     Converts a PowerShell object to a data table row using PSPropertyExpression to evaluate property accessors.
    /// </summary>
    public static DataTableRow CastObjectToDataTableRow(PSObject psObject, List<DataTableColumn> dataTableColumns,
        int objectIndex)
    {
        var valuePairs = new Dictionary<string, IValue>();

        foreach (var column in dataTableColumns)
        {
            object? result;

            try
            {
                // PSPropertyExpression constructor takes a ScriptBlock for script expressions
                // For simple properties, we need to extract just the property name
                var accessor = column.PropertyScriptAccessor;

#pragma warning disable CA1310
                if (accessor.StartsWith("$_.\"") && accessor.EndsWith('"'))
#pragma warning restore CA1310
                {
                    // Extract property name from "$_."PropertyName"" format
                    var propertyName = accessor.Substring(4, accessor.Length - 5);
                    var property = psObject.Properties[propertyName];
                    result = property?.Value;

                    // Unwrap PSObject if needed to get the base value
                    if (result is PSObject psObjResult) result = psObjResult.BaseObject;
                }
                else if (accessor == "$_")
                {
                    // The whole object
                    result = psObject.BaseObject;
                }
                else
                {
                    // It's a script block - create and invoke it
                    var scriptBlock = ScriptBlock.Create(accessor);
                    var results = scriptBlock.InvokeWithContext(null, [new("_", psObject)]);
                    result = results.FirstOrDefault();

                    // Unwrap PSObject if needed
                    if (result is PSObject psScriptResult) result = psScriptResult.BaseObject;
                }
            }
            catch (Exception)
            {
                // If evaluation fails, use null
                result = null;
            }

            // Convert to string and strip ANSI codes
            var displayValue = result?.ToString() ?? string.Empty;
            displayValue = StripAnsiCodes(displayValue);

            // Determine if this is a numeric value for sorting
            var isNumeric = result is decimal or int or long or short or byte or double or float or uint or ulong
                or ushort or sbyte;

            var columnKey = column.ToString();

            if (isNumeric)
            {
                var decimalValue = Convert.ToDecimal(result, CultureInfo.InvariantCulture);
                valuePairs[columnKey] = new DecimalValue
                {
                    DisplayValue = displayValue,
                    SortValue = decimalValue
                };
            }
            else
            {
                valuePairs[columnKey] = new StringValue
                {
                    DisplayValue = displayValue,
                    RawValue = result
                };
            }
        }

        return new DataTableRow(valuePairs, objectIndex);
    }

    /// <summary>
    ///     Sets the data type on each column based on the values in the data rows.
    /// </summary>
    private static void SetTypesOnDataColumns(List<DataTableRow> dataTableRows, List<DataTableColumn> dataTableColumns)
    {
        var dataRows = dataTableRows.Select(x => x.Values);

        foreach (var dataColumn in dataTableColumns)
            dataColumn.StringType = typeof(decimal).FullName;

        // If every value in a column could be a decimal, assume that it is supposed to be a decimal
        foreach (var dataRow in dataRows)
        foreach (var dataColumn in dataTableColumns)
            if (dataRow[dataColumn.ToString()] is not DecimalValue)
                dataColumn.StringType = typeof(string).FullName;
    }

    /// <summary>
    ///     Converts a list of PowerShell objects into a data table structure suitable for display in the grid view.
    /// </summary>
    /// <param name="psObjects">The list of PowerShell objects to convert.</param>
    public static DataTable CastObjectsToTableView(List<PSObject> psObjects)
    {
        if (psObjects.Count == 0) return new DataTable([], []);

        // Get the columns using format view definitions
        var typeGetter = new TypeGetter();
        var dataTableColumns = typeGetter.GetDataColumnsForObject(psObjects).ToList();

        // Convert each object to a row
        var dataTableRows = new List<DataTableRow>();
        for (var i = 0; i < psObjects.Count; i++)
        {
            var dataTableRow = CastObjectToDataTableRow(psObjects[i], dataTableColumns, i);
            dataTableRows.Add(dataTableRow);
        }

        SetTypesOnDataColumns(dataTableRows, dataTableColumns);

        return new DataTable(dataTableColumns, dataTableRows);
    }
}