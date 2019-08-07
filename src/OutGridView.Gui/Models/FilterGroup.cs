using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using DynamicData.Binding;
using ReactiveUI;
using OutGridView.Models;


namespace OutGridView.Application.Models
{
    public class FilterGroup : ReactiveObject
    {
        public FilterGroup(DataTableColumn key, IEnumerable<Filter> items)
        {
            this.Key = key;
            this.Items = items;
        }
        public IEnumerable<Filter> Items { get; }
        public DataTableColumn Key { get; }
    }
}