// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using OutGridView.Application.Models;
using OutGridView.Application.Services.FilterOperators;
using System.Collections.ObjectModel;
using DynamicData;
using OutGridView.Models;


namespace OutGridView.Application.Services
{
    static class FilterBuilder
    {
        public static Func<DataTableRow, bool> BuildFilter(string searchText, IObservableList<FilterGroup> filters)
        {
            var filterQuery = BuildFilterQuery(filters);
            var quickSearchFilter = BuildQuickSearchFilter(searchText);
            return dataList => filterQuery(dataList) && quickSearchFilter(dataList);
        }
        public static Func<DataTableRow, bool> BuildFilterQuery(IObservableList<FilterGroup> filterGroups)
        {

            return dataList => filterGroups.Items.All(filterGroup =>
            {
                return filterGroup.Filters.Any(f =>
                {
                    //Empty filter is always valid (if it requires a value)
                    if (f.SelectedFilterOperator.HasValue && String.IsNullOrEmpty(f.Value))
                    {
                        return true;
                    }
                    var rule = f.SelectedFilterOperator;
                    var value = dataList.Values[f.DataColumn.ToString()];
                    return rule.Execute(value?.DisplayValue ?? String.Empty);
                });
            });
        }

        public static Func<DataTableRow, bool> BuildQuickSearchFilter(string searchText)
        {
            List<string> tokens = ParseSearchText(searchText);

            if (string.IsNullOrEmpty(searchText))
            {
                return dataList => true;
            }

            //For all terms at least-one property matches
            return dataList => tokens.All(t =>
            {
                return dataList.Values.Any(data =>
                    {
                        //Quick Search is NOT case-sensitive
                        return data.Value != null && data.Value.DisplayValue.ToLowerInvariant().Contains(t.ToLowerInvariant());
                    });
            });
        }
        public static string TokenPattern = @"[^\s""']+|""([^""]*)""|'([^']*)'";
        //Seperates words by spaces unless they are quoted
        public static List<string> ParseSearchText(string searchText)
        {
            RegexOptions options = RegexOptions.Multiline;

            List<string> stringMatches = new List<string>();

            foreach (Match m in Regex.Matches(searchText, TokenPattern, options))
            {
                string token;

                if (m.Groups[1].Value != String.Empty)
                {
                    token = m.Groups[1].Value;
                }
                else if (m.Groups[2].Value != String.Empty)
                {
                    token = m.Groups[2].Value;
                }
                else
                {
                    token = m.Value;
                }

                stringMatches.Add(token);
            }
            return stringMatches;
        }
    }
}


