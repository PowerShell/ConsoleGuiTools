using System.Reflection;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Linq;
using System.Collections.Generic;

namespace OutGridView.Models
{
    public class DataTableColumn
    {
        public string Label { get; set; }
        public Type Type { get; set; }
        public int Index { get; set; }
        public DataTableColumn(string label, int index, Type type)
        {
            Label = label;
            Index = index;
            Type = type;
        }
    }
}