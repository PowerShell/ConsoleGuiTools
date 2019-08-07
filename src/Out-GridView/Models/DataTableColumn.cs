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
        public int Index { get; set; }
        public DataTableColumn(string label, int index, string stringType)
        {
            Label = label;
            Index = index;
            StringType = stringType;
        }
    }
}