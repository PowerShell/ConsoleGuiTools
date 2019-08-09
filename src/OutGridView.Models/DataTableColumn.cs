// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
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
        public int Index { get; set; }
        public DataTableColumn(string label, int index, string stringType, string propertyScriptAccessor)
        {
            Label = label;
            Index = index;
            StringType = stringType;
            PropertyScriptAccessor = propertyScriptAccessor;
        }
    }
}
