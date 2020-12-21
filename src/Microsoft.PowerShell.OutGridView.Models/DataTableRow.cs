// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace OutGridView.Models
{
    public interface IValue : IComparable
    {
        string DisplayValue { get; set; }
    }
    public class DecimalValue : IValue
    {
        public string DisplayValue { get; set; }
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
        public string DisplayValue { get; set; }
        public int CompareTo(object obj)
        {
            StringValue otherStringValue = obj as StringValue;
            if (otherStringValue == null) return 1;
            return DisplayValue.CompareTo(otherStringValue.DisplayValue);
        }
    }
    public class DataTableRow
    {
        //key is datacolumn hash code
        //have to do it this way because JSON can't serialize objects as keys
        public Dictionary<string, IValue> Values { get; set; }
        public int OriginalObjectIndex { get; set; }
        public DataTableRow(Dictionary<string, IValue> data, int originalObjectIndex)
        {
            Values = data;
            OriginalObjectIndex = originalObjectIndex;
        }
    }
}
