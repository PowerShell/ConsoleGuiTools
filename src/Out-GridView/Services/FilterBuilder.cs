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
        public static Func<PSObject, bool> BuildFilter(string searchText, IEnumerable<PropertyInfo> properties, IObservableList<FilterGroup> filters)
        {
            var filterQuery = BuildFilterQuery(filters);
            var quickSearchFilter = BuildQuickSearchFilter(searchText, properties);
            return PSObject => filterQuery(PSObject) && quickSearchFilter(PSObject);
        }
        public static Func<PSObject, bool> BuildFilterQuery(IObservableList<FilterGroup> filterGroups)
        {

            return PSObject => filterGroups.Items.All(filterGroup =>
            {
                return filterGroup.Items.Any(f =>
                {
                    var rule = FilterOperatorLookup.CreateFilterOperatorRule(f);
                    var value = f.Property.GetValue(PSObject.BaseObject, null);
                    return rule.Execute(value == null ? String.Empty : value.ToString());
                });
            });
        }

        public static Func<PSObject, bool> BuildQuickSearchFilter(string searchText, IEnumerable<PropertyInfo> properties)
        {
            List<string> tokens = ParseSearchText(searchText.ToLowerInvariant());

            if (string.IsNullOrEmpty(searchText))
            {
                return PSObject => true;
            }

            //For all terms at least-one property matches
            return PSObject => tokens.All(t =>
            {
                return properties.Any(p =>
                          {
                              var value = p.GetValue(PSObject.BaseObject, null);
                              return value != null && value.ToString().ToLowerInvariant().Contains(t);
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


