// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Reflection;
using Terminal.Gui;
using Terminal.Gui.Trees;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Linq;

namespace OutGridView.Cmdlet
{
    internal class ShowObjectView : Window, ITreeBuilder<object>
    {
        private readonly TreeView<object> tree;

        public bool SupportsCanExpand => true;

        public ShowObjectView(List<object> rootObjects)
        {
            Width = Dim.Fill();
            Height = Dim.Fill();

            tree = new TreeView<object>
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };
            tree.TreeBuilder = this;

            if (rootObjects.Count > 0)
            {
                tree.AddObjects(rootObjects);
            }
            else
            {
                tree.AddObject("No Objects");
            }

            Add(tree);
        }

        

        public bool CanExpand(object toExpand)
        {
            if (toExpand is CachedMemberResult p)
            {
                return IsBasicType(p?.Value);
            }

            // Any complex object type can be expanded to reveal properties
            return IsBasicType(toExpand);
        }

        private bool IsBasicType(object? value)
        {
            return value != null && value is not string && !value.GetType().IsValueType;
        }

        public IEnumerable<object> GetChildren(object forObject)
        {
            if(forObject is CachedMemberResult p) 
            {
                return GetChildren(p.Value);
            }

            List<object> children = new List<object>();
            
            // Vanilla object
            foreach (var prop in forObject.GetType().GetProperties())
            {
                children.Add(new CachedMemberResult(forObject, prop));
            }
            foreach (var field in forObject.GetType().GetFields())
            {
                children.Add(new CachedMemberResult(forObject, field));
            }

            return children;
        }

        internal static void Run(List<PSObject> objects)
        {

            Application.Init();
            Window window = null;
            
            try
            {                
                window = new ShowObjectView(objects.Select(p=>p.BaseObject).ToList());
                Application.Run(window);
            }
            finally{
                Application.Shutdown();
                window?.Dispose();
            }
        }

        class CachedMemberResult
        {
            public MemberInfo Member;
            public object Value;
            public object Parent;
            private string representation;

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
                        throw new NotSupportedException($"Unknown {nameof(MemberInfo)} Type");

                    representation = Value?.ToString() ?? "Null";

                }
                catch (Exception)
                {
                    Value = representation = "Unavailable";
                }
            }

            public override string ToString()
            {
                return Member.Name + ":" + representation;
            }
        }
    }
}
