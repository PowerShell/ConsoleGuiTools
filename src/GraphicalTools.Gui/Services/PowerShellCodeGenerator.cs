// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using OutGridView.Application.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace OutGridView.Application.Services
{
    public static class PowerShellCodeGenerator
    {
        public static string GetPowershellForFilterGroups(IEnumerable<FilterGroup> filterGroups)
        {
            var whereObjectClauses = filterGroups.Select(filterGroup =>
            {
                var operatorStrings = filterGroup.Filters.Select(filter =>
                {
                    var powerShellString = filter.SelectedFilterOperator.GetPowerShellString();
                    return $" {filterGroup.DataColumn.PropertyScriptAccessor} {powerShellString} ";
                });

                var operatorString = String.Join("-or", operatorStrings);
                return $"Where-Object {{ {operatorString} }}";
            });

            //New line join for readability
            return String.Join(" |" + System.Environment.NewLine, whereObjectClauses);
        }
        public static string EscapePowerShellLikeString(string str)
        {
            //PowerShell like has special escape characters
            var charsToEscape = new List<string> { "[", "]", "?", "*" };
            foreach (var character in charsToEscape)
            {
                str = str.Replace(character, "`" + character);
            }
            return str;
        }
    }
}
