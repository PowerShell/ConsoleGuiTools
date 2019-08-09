// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
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
        public List<IValue> Values { get; set; }
        public int OriginalObjectIndex { get; set; }
        public DataTableRow(List<IValue> data, int originalObjectIndex)
        {
            Values = data;
            OriginalObjectIndex = originalObjectIndex;
        }
    }
}
