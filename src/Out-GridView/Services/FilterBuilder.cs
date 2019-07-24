using System;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using OutGridView.Models;
using OutGridView.Services.FilterOperators;
using System.Collections.ObjectModel;
using DynamicData;

namespace OutGridView.Services
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
                return filterGroup.Items.Any(f =>
                {
                    //Empty filter is always valid (if it requires a value)
                    if (f.SelectedFilterOperator.HasValue && String.IsNullOrEmpty(f.Value))
                    {
                        return true;
                    }
                    var rule = f.SelectedFilterOperator;
                    var value = dataList.Data[f.DataColumn.Index];
                    return rule.Execute(value == null ? String.Empty : value.Value);
                });
            });
        }

        public static Func<DataTableRow, bool> BuildQuickSearchFilter(string searchText)
        {
            List<string> tokens = ParseSearchText(searchText.ToLowerInvariant());

            if (string.IsNullOrEmpty(searchText))
            {
                return dataList => true;
            }

            //For all terms at least-one property matches
            return dataList => tokens.All(t =>
            {
                return dataList.Data.Any(data =>
                    {
                        return data != null && data.Value.ToLowerInvariant().Contains(t);
                    });
            });
        }
        public static string TokenPattern = @"[^\s""']+|""([^""]*)""|'([^']*)'";
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


