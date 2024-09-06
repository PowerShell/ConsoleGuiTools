// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OutGridView.Cmdlet.TreeNodeCaching
{
    abstract class CachedMemberResultBase : ICachedMemberResult
    {
        public object Value {get; protected set;}
        public object Parent;
        protected string Representation;
        private List<CachedMemberResultElement> valueAsList;


        public bool IsCollection => valueAsList != null;
        public IReadOnlyCollection<CachedMemberResultElement> Elements => valueAsList?.AsReadOnly();


        protected string ValueToString()
        {
            if (Value == null)
            {
                return "Null";
            }
            try
            {
                if (IsCollectionOfKnownTypeAndSize(out Type elementType, out int size))
                {
                    return $"{elementType.Name}[{size}]";
                }
            }
            catch (Exception)
            {
                return Value?.ToString();
            }


            return Value?.ToString();
        }

        private bool IsCollectionOfKnownTypeAndSize(out Type elementType, out int size)
        {
            elementType = null;
            size = 0;

            if (Value == null || Value is string)
            {

                return false;
            }

            if (Value is IEnumerable ienumerable)
            {
                var list = ienumerable.Cast<object>().ToList();

                var types = list.Where(v => v != null).Select(v => v.GetType()).Distinct().ToArray();

                if (types.Length == 1)
                {
                    elementType = types[0];
                    size = list.Count;

                    valueAsList = list.Select((e, i) => new CachedMemberResultElement(e, i)).ToList();
                    return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return GetMemberName() + ": " + Representation;
        }

        protected abstract string GetMemberName();
    }
}
