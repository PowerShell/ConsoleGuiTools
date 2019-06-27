using System;
using DynamicData;
using System.Management.Automation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reflection;

namespace OutGridView.Models
{
    public class DataSource : ReactiveObject
    {
        public SourceList<PSObject> PSObjects { get; set; } = new SourceList<PSObject>();
        public IObservableList<PropertyInfo> PSObjectProperties { get; }
        [Reactive] public string SearchText { get; set; }
        public DataSource(IEnumerable<PSObject> PsObjects)
        {
            PSObjects.AddRange(PsObjects);

            PSObjectProperties = PSObjects.Connect()
              .TransformMany(x => x.BaseObject.GetType().GetProperties())
              .DistinctValues(x => x)
              .AsObservableList();
        }
    }
}