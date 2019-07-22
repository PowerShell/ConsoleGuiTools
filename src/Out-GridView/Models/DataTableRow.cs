using System;
using System.Management.Automation;
using System.Collections.Generic;

namespace OutGridView.Models
{
    public class DataTableRow
    {
        public List<string> Data { get; }
        public PSObject OriginalObject { get; }
        public DataTableRow(List<string> data, PSObject originalObject)
        {
            Data = data;
            OriginalObject = originalObject;
        }
    }
}