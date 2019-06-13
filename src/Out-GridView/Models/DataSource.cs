using System;
using DynamicData;
using System.Management.Automation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;
using DynamicData.Binding;
using System.Linq;
using ReactiveUI;
using System.Reflection;

namespace OutGridView.Models
{
    public class DataSource
    {
        public SourceList<PSObject> PSObjects { get; set; } = new SourceList<PSObject>();

        public IEnumerable<PropertyInfo> PSObjectProperties => new List<PropertyInfo>(PSObjects.Items.FirstOrDefault().BaseObject.GetType().GetProperties());


        public SourceList<CriteriaFilter> CriteriaFilters { get; set; }

        [Reactive] public string SearchText { get; set; }

        public DataSource(IEnumerable<PSObject> PsObjects)
        {
            PSObjects.AddRange(PsObjects);
        }
    }
}