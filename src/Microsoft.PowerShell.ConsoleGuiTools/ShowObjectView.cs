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
using System.Diagnostics;
using System.Collections;

namespace OutGridView.Cmdlet
{
    internal class ShowObjectView : Window, ITreeBuilder<object>
    {
        private readonly TreeView<object> tree;
        private readonly TreeViewTextFilter<object> filter;

        public bool SupportsCanExpand => true;
        private StatusItem selectedStatusBarItem;
        private StatusBar statusBar;

        public ShowObjectView(List<object> rootObjects)
        {
            Title = "Show-ObjectTree (Ctrl+Q to quit)";
            Width = Dim.Fill();
            Height = Dim.Fill();

            tree = new TreeView<object>
            {
                Y = 2,
                Width = Dim.Fill(),
                Height = Dim.Fill(1),
            };
            tree.TreeBuilder = this;
            tree.AspectGetter = this.AspectGetter;
            tree.SelectionChanged += this.SelectionChanged;

            tree.ClearKeybinding(Command.ExpandAll);

            this.filter = new TreeViewTextFilter<object>(tree);
            tree.Filter = this.filter;

            if (rootObjects.Count > 0)
            {
                tree.AddObjects(rootObjects);
            }
            else
            {
                tree.AddObject("No Objects");
            }
            statusBar = new StatusBar();
            
            string elementDescription = "objects";

            var types = rootObjects.Select(o=>o.GetType()).Distinct().ToArray();
            if(types.Length == 1)
            {
                elementDescription = types[0].Name;
            }

            var lblFilter = new Label(){
                Text = "Filter:"
            };
            var tbFilter = new TextField(){
                X = Pos.Right(lblFilter),
                Width = Dim.Fill()
            };
            tbFilter.TextChanged += (_)=>{
                filter.Text = tbFilter.Text.ToString();
            };

            Add(lblFilter);
            Add(tbFilter);

            tbFilter.FocusFirst();

            var siCount = new StatusItem(Key.Null, $"{rootObjects.Count} {elementDescription}",null);
            selectedStatusBarItem = new StatusItem(Key.Null, string.Empty,null);
            statusBar.AddItemAt(0,siCount);
            statusBar.AddItemAt(1,selectedStatusBarItem);
            Add(statusBar);
            Add(tree);
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs<object> e)
        {
            var selectedValue = e.NewValue;
            
            if( selectedValue is CachedMemberResult cmr)
            {
                selectedValue = cmr.Value;
            }

            if(selectedValue != null && selectedStatusBarItem != null)
            {
                selectedStatusBarItem.Title = selectedValue.GetType().Name;
            }
            else
            {
                selectedStatusBarItem.Title = string.Empty;
            }
            
            statusBar.SetNeedsDisplay();
        }

        private string AspectGetter(object toRender)
        {
            if(toRender is Process p)
            {
                return p.ProcessName;
            }
            if(toRender is null)
            {
                return "Null";
            }

            return toRender.ToString();
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

        private bool IsBasicType(object value)
        {
            return value != null && value is not string && !value.GetType().IsValueType;
        }

        public IEnumerable<object> GetChildren(object forObject)
        {
            if(forObject is CachedMemberResult p) 
            {
                if(p.IsCollection)
                {
                    return p.Elements;
                }

                return GetChildren(p.Value);
            }

            if(forObject is CachedMemberResultElement e)
            {
                return GetChildren(e.Value);
            }

            List<object> children = new List<object>();
            
            foreach(var member in forObject.GetType().GetMembers().OrderBy(m=>m.Name))
            {
                if(member is PropertyInfo prop)
                {
                    children.Add(new CachedMemberResult(forObject, prop));
                }
                if(member is FieldInfo field)
                {
                    children.Add(new CachedMemberResult(forObject, field));
                }
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

        class CachedMemberResultElement
        {
            public int Index;
            public object Value;

            private string representation;

            public CachedMemberResultElement(object value, int index)
            {
                Index = index;
                Value = value;

                try{
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

        class CachedMemberResult
        {
            public MemberInfo Member;
            public object Value;
            public object Parent;
            private string representation;            
            private List<CachedMemberResultElement> valueAsList;

            
            public bool IsCollection => valueAsList != null;
            public IReadOnlyCollection<CachedMemberResultElement> Elements => valueAsList?.AsReadOnly();

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
                    
                    representation = ValueToString();

                }
                catch (Exception)
                {
                    Value = representation = "Unavailable";
                }
            }

            private string ValueToString()
            {
                if(Value == null)
                {
                    return "Null";
                }
                try{
                    if(IsCollectionOfKnownTypeAndSize(out Type elementType, out int size))
                    {
                        return $"{elementType.Name}[{size}]";
                    }
                }catch(Exception)
                {
                    return Value?.ToString();
                }
                

                return Value?.ToString();
            }

            private bool IsCollectionOfKnownTypeAndSize(out Type elementType, out int size)
            {
                elementType = null;
                size = 0;

                if(Value == null || Value is string)
                {
                    
                    return false;
                }

                if(Value is IEnumerable ienumerable)
                {
                    var list = ienumerable.Cast<object>().ToList();

                    var types = list.Where(v=>v!=null).Select(v=>v.GetType()).Distinct().ToArray();

                    if(types.Length == 1)
                    {
                        elementType = types[0];
                        size = list.Count;

                        valueAsList = list.Select((e,i)=>new CachedMemberResultElement(e,i)).ToList();
                        return true;
                    }
                }
                
                return false;
            }

            public override string ToString()
            {
                return Member.Name + ": " + representation;
            }
        }
    }
}
