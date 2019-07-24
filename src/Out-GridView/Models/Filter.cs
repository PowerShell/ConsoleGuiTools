using System.Reflection;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using OutGridView.Services.FilterOperators;

namespace OutGridView.Models
{
    public class Filter : ReactiveObject
    {
        public static IEnumerable<StringFilterOperator> Operators { get; } = Enum.GetValues(typeof(StringFilterOperator)).Cast<StringFilterOperator>();
        public DataTableColumn DataColumn { get; set; }
        [Reactive] public StringFilterOperator SelectedOperator { get; set; }

        public IStringFilterOperator SelectedFilterOperator { [ObservableAsProperty] get; }
        [Reactive] public string Value { get; set; }
        public Filter(DataTableColumn dataColumn)
        {
            this.WhenAnyValue(x => x.SelectedOperator, x => x.Value, (op, value) => FilterOperatorLookup.CreateFilterOperatorRule(op, value))
                .ToPropertyEx(this, x => x.SelectedFilterOperator, FilterOperatorLookup.CreateFilterOperatorRule(SelectedOperator, Value));

            DataColumn = dataColumn;
            SelectedOperator = StringFilterOperator.Contains;
            Value = string.Empty;
        }
    }
}