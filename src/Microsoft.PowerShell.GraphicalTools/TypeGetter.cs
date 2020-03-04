// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Management.Automation;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.PowerShell.Commands;
using OutGridView.Models;

namespace OutGridView.Cmdlet
{
    public class TypeGetter
    {
        private PSCmdlet _cmdlet;

        public TypeGetter(PSCmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }
        public FormatViewDefinition GetFormatViewDefinitionForObject(PSObject obj)
        {
            var typeName = obj.BaseObject.GetType().FullName;

            var types = _cmdlet.InvokeCommand.InvokeScript(@"Microsoft.PowerShell.Utility\Get-FormatData " + typeName).ToList();

            //No custom type definitions found - try the PowerShell specific format data
            if (types == null || types.Count == 0) 
            {
                types = _cmdlet.InvokeCommand
                    .InvokeScript(@"Microsoft.PowerShell.Utility\Get-FormatData -PowerShellVersion $PSVersionTable.PSVersion " + typeName).ToList();

                if (types == null || types.Count == 0)
                {
                    return null;
                }
            }

            var extendedTypeDefinition = types[0].BaseObject as ExtendedTypeDefinition;

            return extendedTypeDefinition.FormatViewDefinition[0];
        }

        public DataTableRow CastObjectToDataTableRow(PSObject ps, List<DataTableColumn> dataColumns, int objectIndex)
        {
            Dictionary<string, IValue> valuePairs = new Dictionary<string, IValue>();

            foreach (var dataColumn in dataColumns)
            {
                var expression = new PSPropertyExpression(ScriptBlock.Create(dataColumn.PropertyScriptAccessor));

                var result = expression.GetValues(ps).FirstOrDefault().Result;

                var stringValue = result?.ToString() ?? String.Empty;

                var isDecimal = decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var decimalValue);

                if (isDecimal)
                {
                    valuePairs[dataColumn.ToString()] = new DecimalValue { DisplayValue = stringValue, SortValue = decimalValue };
                }
                else
                {
                    valuePairs[dataColumn.ToString()] = new StringValue { DisplayValue = stringValue };
                }
            }

            return new DataTableRow(valuePairs, objectIndex);
        }

        private void SetTypesOnDataColumns(List<DataTableRow> dataTableRows, List<DataTableColumn> dataTableColumns)
        {
            var dataRows = dataTableRows.Select(x => x.Values);

            foreach (var dataColumn in dataTableColumns)
            {
                dataColumn.StringType = typeof(decimal).FullName;
            }

            //If every value in a column could be a decimal, assume that it is supposed to be a decimal
            foreach (var dataRow in dataRows)
            {
                foreach (var dataColumn in dataTableColumns)
                {
                    if (!(dataRow[dataColumn.ToString()] is DecimalValue))
                    {
                        dataColumn.StringType = typeof(string).FullName;
                    }
                }
            }
        }
        private List<DataTableColumn> GetDataColumnsForObject(List<PSObject> psObjects)
        {
            var dataColumns = new List<DataTableColumn>();



            foreach (PSObject obj in psObjects)
            {
                var labels = new List<string>();

                FormatViewDefinition fvd = GetFormatViewDefinitionForObject(obj);

                var propertyAccessors = new List<string>();

                if (fvd == null)
                {
                    if (PSObjectIsPrimitive(obj))
                    {
                        labels = new List<string> { obj.BaseObject.GetType().Name };
                        propertyAccessors = new List<string> { "$_" };
                    }
                    else
                    {
                        labels = obj.Properties.Select(x => x.Name).ToList();
                        propertyAccessors = obj.Properties.Select(x => $"$_.\"{x.Name}\"").ToList();
                    }
                }
                else
                {
                    var tableControl = fvd.Control as TableControl;

                    var definedColumnLabels = tableControl.Headers.Select(x => x.Label);

                    var displayEntries = tableControl.Rows[0].Columns.Select(x => x.DisplayEntry);

                    var propertyLabels = displayEntries.Select(x => x.Value);

                    //Use the TypeDefinition Label if availble otherwise just use the property name as a label
                    labels = definedColumnLabels.Zip(propertyLabels, (definedColumnLabel, propertyLabel) =>
                    {
                        if (String.IsNullOrEmpty(definedColumnLabel))
                        {
                            return propertyLabel;
                        }
                        return definedColumnLabel;
                    }).ToList();


                    propertyAccessors = displayEntries.Select(x =>
                       {
                           //If it's a propety access directly
                           if (x.ValueType == DisplayEntryValueType.Property)
                           {
                               return $"$_.\"{x.Value}\"";
                           }
                           //Otherwise return access script
                           return x.Value;
                       }).ToList();
                }

                for (var i = 0; i < labels.Count; i++)
                {
                    dataColumns.Add(new DataTableColumn(labels[i], propertyAccessors[i]));
                }
            }
            return dataColumns.Distinct().ToList();
        }

        public DataTable CastObjectsToTableView(List<PSObject> psObjects)
        {
            List<FormatViewDefinition> objectFormats = psObjects.Select(GetFormatViewDefinitionForObject).ToList();

            var dataTableColumns = GetDataColumnsForObject(psObjects);

            foreach (var dataColumn in dataTableColumns)
            {
                _cmdlet.WriteVerbose(dataColumn.ToString());
            }

            List<DataTableRow> dataTableRows = new List<DataTableRow>();
            for (var i = 0; i < objectFormats.Count; i++)
            {
                var dataTableRow = CastObjectToDataTableRow(psObjects[i], dataTableColumns, i);
                dataTableRows.Add(dataTableRow);
            }

            SetTypesOnDataColumns(dataTableRows, dataTableColumns);

            return new DataTable(dataTableColumns, dataTableRows);
        }


        //Types that are condisidered primitives to PowerShell but not C#
        private readonly static List<string> additionalPrimitiveTypes = new List<string> { "System.String",
            "System.Decimal",
            "System.IntPtr",
            "System.Security.SecureString",
            "System.Numerics.BigInteger"
        };
        private bool PSObjectIsPrimitive(PSObject ps)
        {
            var psBaseType = ps.BaseObject.GetType();

            return psBaseType.IsPrimitive || psBaseType.IsEnum || additionalPrimitiveTypes.Contains(psBaseType.FullName);
        }
    }
}
