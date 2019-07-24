using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.PowerShell.Commands;
using OutGridView.Models;

namespace OutGridView.Services
{
    public class TypeGetter
    {
        private PowerShell _powerShell;

        public TypeGetter(PowerShell powerShellInstance)
        {
            _powerShell = powerShellInstance;

            Runspace rs = RunspaceFactory.CreateRunspace();
            rs.Open();
            _powerShell.Runspace = rs;

            Runspace.DefaultRunspace = rs;
        }

        public FormatViewDefinition GetFormatViewDefinitonForObject(PSObject obj)
        {
            var typeName = obj.BaseObject.GetType().FullName;

            var types = _powerShell.AddScript("Get-FormatData " + typeName)
                .Invoke<PSObject>();

            var extendedTypeDefiniton = types[0].BaseObject as ExtendedTypeDefinition;

            return extendedTypeDefiniton.FormatViewDefinition[0];
        }

        public static DataTableRow CastObjectToDataTableRow(PSObject ps, FormatViewDefinition fvd)
        {
            var tableControl = fvd.Control as TableControl;

            var columns = tableControl.Rows[0].Columns;

            var PSObject = new PSObject(ps.BaseObject);

            var expressions = new List<PSPropertyExpression>();

            foreach (var column in columns)
            {
                var displayEntry = column.DisplayEntry;
                if (displayEntry.ValueType == DisplayEntryValueType.Property)
                {
                    expressions.Add(new PSPropertyExpression(displayEntry.Value));
                }
                if (displayEntry.ValueType == DisplayEntryValueType.ScriptBlock)
                {
                    expressions.Add(new PSPropertyExpression(ScriptBlock.Create(displayEntry.Value)));
                }
            }

            var data = expressions.Select(x => x.GetValues(ps).FirstOrDefault().Result.ToString())
                .Select<string, IValue>(x =>
                {
                    var isDecimal = decimal.TryParse(x, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var decimalValue);
                    if (isDecimal)
                    {
                        return new DecimalValue
                        {
                            Value = x,
                            SortValue = decimalValue
                        };
                    }
                    return new StringValue
                    {
                        Value = x
                    };
                }).ToList();

            return new DataTableRow(data, ps);
        }

        public static List<Type> GetTypesForColumns(List<DataTableRow> dataTableRows)
        {
            var dataRows = dataTableRows.Select(x => x.Data);
            var types = dataRows.FirstOrDefault().Select(x => typeof(decimal)).ToList();

            foreach (var dataRow in dataRows)
            {
                for (var i = 0; i < dataRow.Count; i++)
                {
                    var isNumber = dataRow.ElementAt(i) is DecimalValue;
                    if (!isNumber) types[i] = typeof(string);
                }
            }
            return types;
        }
        public static List<DataTableColumn> GetColumnHeadersForObject(FormatViewDefinition fvd, List<Type> types)
        {

            var tableControl = fvd.Control as TableControl;

            var definedColumnLabels = tableControl.Headers.Select(x => x.Label);

            var propertyLabels = tableControl.Rows[0].Columns.Select(x => x.DisplayEntry.Value);

            return definedColumnLabels.Zip(propertyLabels, (definedColumnLabel, propertyLabel) =>
            {
                if (String.IsNullOrEmpty(definedColumnLabel))
                {
                    return propertyLabel;
                }
                return definedColumnLabel;
            }).Zip(types, (definedColumnLabel, type) => (definedColumnLabel, type))
            .Select((labelTypePair, i) => new DataTableColumn(labelTypePair.definedColumnLabel, i, labelTypePair.type))
            .ToList();
        }

        public static DataTable CastObjectsToTableView(List<PSObject> psObjects, FormatViewDefinition fvd)
        {
            var formattedObjects = psObjects.Select(ps => CastObjectToDataTableRow(ps, fvd)).ToList();

            var columnTypes = GetTypesForColumns(formattedObjects);

            var columnHeaders = GetColumnHeadersForObject(fvd, columnTypes);

            return new DataTable(columnHeaders, formattedObjects);
        }
    }
}