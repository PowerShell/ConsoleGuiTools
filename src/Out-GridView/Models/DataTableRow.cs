using System;
using System.Management.Automation;
using System.Collections.Generic;

namespace OutGridView.Models
{
    public interface IValue : IComparable
    {
        string Value { get; set; }
    }
    public class DecimalValue : IValue
    {
        public string Value { get; set; }
        public decimal SortValue { get; set; }

        public int CompareTo(object obj)
        {
            DecimalValue otherDecimalValue = obj as DecimalValue;
            if (otherDecimalValue == null) return 1;
            return Decimal.Compare(SortValue, otherDecimalValue.SortValue);
        }
    }
    public class StringValue : IValue
    {
        public string Value { get; set; }
        public int CompareTo(object b)
        {
            StringValue castB = b as StringValue;
            if (castB == null) return 1;
            return Value.CompareTo(castB.Value);
        }
    }
    public class DataTableRow
    {
        public List<IValue> Data { get; }
        public PSObject OriginalObject { get; }
        public DataTableRow(List<IValue> data, PSObject originalObject)
        {
            Data = data;
            OriginalObject = originalObject;
        }
    }
}