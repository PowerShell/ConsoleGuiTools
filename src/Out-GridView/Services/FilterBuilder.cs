using System;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using OutGridView.Models;
using OutGridView.Services.CriteriaFilters;



namespace OutGridView.Services
{
    static class FilterBuilder
    {
        public static Func<PSObject, bool> BuildFilter(string searchText, IEnumerable<PropertyInfo> properties, IEnumerable<CriteriaFilter> criteriaFilters)
        {
            var criteriaFilter = BuildCriteriaFilter(criteriaFilters);
            var quickSearchFilter = BuildQuickSearchFilter(searchText, properties);
            return PSObject => criteriaFilter(PSObject) && quickSearchFilter(PSObject);
        }
        public static Func<PSObject, bool> BuildCriteriaFilter(IEnumerable<CriteriaFilter> criteriaFilters)
        {

            return PSObject => criteriaFilters.All(f =>
            {
                var rule = CriteriaFilterLookup.CreateCriteriaFilterRule(f);
                var value = f.Property.GetValue(PSObject.BaseObject, null);
                return value != null && rule.Execute(value.ToString());
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

                if (m.Groups.ElementAt(1).Value != String.Empty)
                {
                    token = m.Groups.ElementAt(1).Value;
                }
                else if (m.Groups.ElementAt(2).Value != String.Empty)
                {
                    token = m.Groups.ElementAt(2).Value;
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


