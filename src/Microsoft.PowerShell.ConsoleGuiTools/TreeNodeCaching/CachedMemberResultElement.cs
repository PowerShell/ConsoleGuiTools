// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace OutGridView.Cmdlet.TreeNodeCaching
{
    sealed class CachedMemberResultElement
    {
        public int Index;
        public object Value;

        private string representation;

        public CachedMemberResultElement(object value, int index)
        {
            Index = index;
            Value = value;

            try
            {
                representation = Value?.ToString() ?? "Null";
            }
            catch (Exception)
            {
                Value = representation = "Unavailable";
            }
        }
        public override string ToString()
        {
            return $"[{Index}]: {representation}]";
        }
    }
}
