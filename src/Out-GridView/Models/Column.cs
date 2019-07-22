using System.Reflection;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Linq;
using System.Collections.Generic;

namespace OutGridView.Models
{
    public class Column : ReactiveObject
    {
        public DataTableColumn DataColumn { get; set; }
        [Reactive] public Boolean IsVisible { get; set; }
        public Column(DataTableColumn dataColumn)
        {
            DataColumn = dataColumn;
            IsVisible = true;
        }
    }
}