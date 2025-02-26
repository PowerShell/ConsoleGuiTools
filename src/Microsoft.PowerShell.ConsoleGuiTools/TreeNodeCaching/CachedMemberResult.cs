// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Reflection;

namespace OutGridView.Cmdlet.TreeNodeCaching
{

    sealed class CachedMemberResult : CachedMemberResultBase
    {
        MemberInfo Member {get;}

        protected override string GetMemberName()
        {
            return Member.Name;
        }

        public CachedMemberResult(object parent, MemberInfo mem)
        {
            Parent = parent;
            Member = mem;

            try
            {
                if (mem is PropertyInfo p)
                {
                    Value = p.GetValue(parent);
                }
                else if (mem is FieldInfo f)
                {
                    Value = f.GetValue(parent);
                }
                else
                {
                    throw new NotSupportedException($"Unknown {nameof(MemberInfo)} Type");
                }

                Representation = ValueToString();

            }
            catch (Exception)
            {
                Value = Representation = "Unavailable";
            }
        }
    }
}
