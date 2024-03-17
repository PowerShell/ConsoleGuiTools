// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace OutGridView.Cmdlet.TreeNodeCaching
{
    sealed class CachedPSObjectMemberResult : CachedMemberResultBase<PSMemberInfo>
    {
        public CachedPSObjectMemberResult(object parent, PSMemberInfo mem)
        {
            Parent = parent;
            Member = mem;
            
            if(mem.Value is PSObject psoVal)
            {
                Value = PsoHelper.MaybeUnwrap(psoVal);
            }
            else
            {
                Value = mem.Value;
            }
            

            Representation = ValueToString();
        }

        protected override string GetMemberName()
        {
            return Member.Name;
        }
    }
}
