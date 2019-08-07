using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.PowerShell.Commands;
using OutGridView.Models;

namespace OutGridView.Cmdlet
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

            //No custom type definitions found
            if (types == null || types.Count == 0) return null;

            var extendedTypeDefiniton = types[0].BaseObject as ExtendedTypeDefinition;

            return extendedTypeDefiniton.FormatViewDefinition[0];
        }

        public static DataTableRow CastObjectToDataTableRow(PSObject ps, FormatViewDefinition fvd, int objectIndex)
        {
            var expressions = new List<PSPropertyExpression>();

            //Just iterate properties if no type def is found
            if (fvd == null)
            {
                foreach (var property in ps.Properties)
                {
                    expressions.Add(new PSPropertyExpression(property.Name));
                }
            }
            else
            {
                var tableControl = fvd.Control as TableControl;

                var columns = tableControl.Rows[0].Columns;

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

            }

            // Stringify expression's results because we don't get the types from ExpressionResult
            var stringData = expressions.Select(x =>
            {
                var result = x.GetValues(ps).FirstOrDefault().Result;
                if (result == null)
                {
                    return string.Empty;
                }
                return result.ToString();
            });

            var data = stringData
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

            return new DataTableRow(data, objectIndex);
        }

        public static List<Type> GetTypesForColumns(List<DataTableRow> dataTableRows)
        {
            var dataRows = dataTableRows.Select(x => x.Values);
            var types = dataRows.FirstOrDefault().Select(x => typeof(decimal)).ToList();

            //If every value in a column could be a decimal, assume that it is supposed to be a decimal
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
        public static List<DataTableColumn> GetColumnHeadersForObject(PSObject ps, FormatViewDefinition fvd, List<Type> types)
        {
            var labels = new List<string>();

            if (fvd == null)
            {
                labels = ps.Properties.Select(x => x.Name).ToList();
            }
            else
            {
                var tableControl = fvd.Control as TableControl;

                var definedColumnLabels = tableControl.Headers.Select(x => x.Label);

                var propertyLabels = tableControl.Rows[0].Columns.Select(x => x.DisplayEntry.Value);

                //Use the TypeDefinition Label if availble otherwise just use the property name as a label
                labels = definedColumnLabels.Zip(propertyLabels, (definedColumnLabel, propertyLabel) =>
                {
                    if (String.IsNullOrEmpty(definedColumnLabel))
                    {
                        return propertyLabel;
                    }
                    return definedColumnLabel;
                }).ToList();
            }

            return labels.Zip(types, (label, type) => (label, type))
               .Select((labelTypePair, i) => new DataTableColumn(labelTypePair.label, i, labelTypePair.type.FullName))
               .ToList();
        }

        public static DataTable CastObjectsToTableView(List<PSObject> psObjects, FormatViewDefinition fvd)
        {
            var formattedObjects = psObjects.Select((ps, idx) => CastObjectToDataTableRow(ps, fvd, idx)).ToList();

            var columnTypes = GetTypesForColumns(formattedObjects);

            var columnHeaders = GetColumnHeadersForObject(psObjects.First(), fvd, columnTypes);

            return new DataTable(columnHeaders, formattedObjects);
        }
    }
}