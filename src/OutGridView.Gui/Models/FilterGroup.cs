// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        public FilterGroup(DataTableColumn dataColumn, IEnumerable<Filter> filters)
        {
            this.DataColumn = dataColumn;
            this.Filters = filters;
        }
        public IEnumerable<Filter> Filters { get; }
        public DataTableColumn DataColumn { get; }
    }
}
