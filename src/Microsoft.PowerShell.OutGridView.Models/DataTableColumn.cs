// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;
using System.Text;

namespace OutGridView.Models
{
    public class DataTableColumn
    {
        [JsonIgnore]
        public Type Type => Type.GetType(StringType);
        public string Label { get; set; }
        //Serializable Version of Type
        public string StringType { get; set; }
        public string PropertyScriptAccessor { get; set; }
        public DataTableColumn(string label, string propertyScriptAccessor)
        {
            Label = label;
            PropertyScriptAccessor = propertyScriptAccessor;
        }

        //Distinct column defined by Label, Prop Accessor
        public override bool Equals(object obj)
        {
            DataTableColumn b = obj as DataTableColumn;
            return b.Label == Label && b.PropertyScriptAccessor == PropertyScriptAccessor;
        }
        public override int GetHashCode()
        {
            return Label.GetHashCode() + PropertyScriptAccessor.GetHashCode();
        }
        public override string ToString()
        {
            //Needs to be encoded to embed safely in xaml
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Label + PropertyScriptAccessor));
        }
    }
}
