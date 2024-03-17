// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using OutGridView.Models;
using Terminal.Gui;
using Terminal.Gui.Trees;
using OutGridView.Cmdlet.TreeNodeCaching;

namespace OutGridView.Cmdlet
{
    internal sealed partial class ShowObjectView : Window, ITreeBuilder<object>
    {
        private readonly TreeView<object> tree;
        private readonly RegexTreeViewTextFilter filter;
        private readonly Label filterErrorLabel;

        public bool SupportsCanExpand => true;
        private StatusItem selectedStatusBarItem;
        private StatusBar statusBar;

        public ShowObjectView(List<object> rootObjects, ApplicationData applicationData)
        {
            Title = applicationData.Title;
            Width = Dim.Fill();
            Height = Dim.Fill(1);
            Modal = false;


            if (applicationData.MinUI)
            {
                Border.BorderStyle = BorderStyle.None;
                Title = string.Empty;
                X = -1;
                Height = Dim.Fill();
            }

            tree = new TreeView<object>
            {
                Y = applicationData.MinUI ? 0 : 2,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };
            tree.TreeBuilder = this;
            tree.AspectGetter = this.AspectGetter;
            tree.SelectionChanged += this.SelectionChanged;

            tree.ClearKeybinding(Command.ExpandAll);

            this.filter = new RegexTreeViewTextFilter(this, tree);
            this.filter.Text = applicationData.Filter ?? string.Empty;
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

            var types = rootObjects.Select(o => o.GetType()).Distinct().ToArray();
            if (types.Length == 1)
            {
                elementDescription = types[0].Name;
            }

            var lblFilter = new Label()
            {
                Text = "Filter:",
                X = 1,
            };
            var tbFilter = new TextField()
            {
                X = Pos.Right(lblFilter),
                Width = Dim.Fill(1),
                Text = applicationData.Filter ?? string.Empty
            };
            tbFilter.CursorPosition = tbFilter.Text.Length;

            tbFilter.TextChanged += (_) =>
            {
                filter.Text = tbFilter.Text.ToString();
            };


            filterErrorLabel = new Label(string.Empty)
            {
                X = Pos.Right(lblFilter) + 1,
                Y = Pos.Top(lblFilter) + 1,
                ColorScheme = Colors.Base,
                Width = Dim.Fill() - lblFilter.Text.Length
            };

            if (!applicationData.MinUI)
            {
                Add(lblFilter);
                Add(tbFilter);
                Add(filterErrorLabel);
            }

            int pos = 0;
            statusBar.AddItemAt(pos++, new StatusItem(Key.Esc, "~ESC~ Close", () => Application.RequestStop()));

            var siCount = new StatusItem(Key.Null, $"{rootObjects.Count} {elementDescription}", null);
            selectedStatusBarItem = new StatusItem(Key.Null, string.Empty, null);
            statusBar.AddItemAt(pos++, siCount);
            statusBar.AddItemAt(pos++, selectedStatusBarItem);

            if (applicationData.Debug)
            {
                statusBar.AddItemAt(pos++, new StatusItem(Key.Null, $" v{applicationData.ModuleVersion}", null));
                statusBar.AddItemAt(pos++, new StatusItem(Key.Null,
                $"{Application.Driver} v{FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(Application)).Location).ProductVersion}", null));
            }

            statusBar.Visible = !applicationData.MinUI;
            Application.Top.Add(statusBar);

            Add(tree);
        }
        private void SetRegexError(string error)
        {
            if (string.Equals(error, filterErrorLabel.Text.ToString(), StringComparison.Ordinal))
            {
                return;
            }
            filterErrorLabel.Text = error;
            filterErrorLabel.ColorScheme = Colors.Error;
            filterErrorLabel.Redraw(filterErrorLabel.Bounds);
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs<object> e)
        {
            var selectedValue = e.NewValue;

            if (selectedValue is ICachedMemberResult cmr)
            {
                selectedValue = cmr.Value;
            }

            if (selectedValue != null && selectedStatusBarItem != null)
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
            if (toRender is Process p)
            {
                return p.ProcessName;
            }
            if (toRender is null)
            {
                return "Null";
            }
            if (toRender is FileSystemInfo fsi && !IsRootObject(fsi))
            {
                return fsi.Name;
            }
            if(toRender is PSObject o)
            {
                return o.ToString();
            }
            

            return toRender.ToString();
        }

        private bool IsRootObject(object o)
        {
            return tree.Objects.Contains(o);
        }

        public bool CanExpand(object toExpand)
        {
            if (toExpand is ICachedMemberResult p)
            {
                return IsBasicType(p?.Value);
            }

            // Any complex object type can be expanded to reveal properties
            return IsBasicType(toExpand);
        }

        private static bool IsBasicType(object value)
        {
            return value != null && value is not string && !value.GetType().IsValueType;
        }

        public IEnumerable<object> GetChildren(object forObject)
        {
            if (forObject == null || !this.CanExpand(forObject))
            {
                return Enumerable.Empty<object>();
            }

            if (forObject is ICachedMemberResult p)
            {
                if (p.IsCollection)
                {
                    return p.Elements;
                }

                return GetChildren(p.Value);
            }

            if (forObject is CachedMemberResultElement e)
            {
                return GetChildren(e.Value);
            }

            if(forObject is PSObject pso)
            {
                return GetPSObjectChildren(pso);
            }

            List<object> children = new List<object>();

            foreach (var member in forObject.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public).OrderBy(m => m.Name))
            {
                if (member is PropertyInfo prop)
                {
                    children.Add(new CachedMemberResult(forObject, prop));
                }
                if (member is FieldInfo field)
                {
                    children.Add(new CachedMemberResult(forObject, field));
                }
            }

            try
            {
                children.AddRange(GetExtraChildren(forObject));
            }
            catch (Exception)
            {
                // Extra children unavailable, possibly security or IO exceptions enumerating children etc
            }

            return children;
        }

        /// <summary>
        /// We only deal with PSObject when there is no native type (e.g. Process).
        /// For example when the PSObject.BaseObject is a PSCustomObject.
        /// </summary>
        /// <param name="pso"></param>
        /// <returns></returns>
        public IEnumerable<object> GetPSObjectChildren(PSObject pso)
        {
            foreach(var m in pso.Members.Where(PsoHelper.IsDisplayableMember))
            {
                yield return new CachedPSObjectMemberResult(pso, m);
            }
        }

        private static IEnumerable<object> GetExtraChildren(object forObject)
        {
            if (forObject is DirectoryInfo dir)
            {
                foreach (var c in dir.EnumerateFileSystemInfos())
                {
                    yield return c;
                }
            }
        }

        internal static void Run(List<PSObject> objects, ApplicationData applicationData)
        {
            // Note, in Terminal.Gui v2, this property is renamed to Application.UseNetDriver, hence
            // using that terminology here.
            Application.UseSystemConsole = applicationData.UseNetDriver;
            Application.Init();
            Window window = null;

            try
            {
                window = new ShowObjectView(objects.Select(PsoHelper.MaybeUnwrap).ToList(), applicationData);
                Application.Top.Add(window);
                Application.Run();
            }
            finally
            {
                Application.Shutdown();
                window?.Dispose();
            }
        }


        private sealed class RegexTreeViewTextFilter : ITreeViewFilter<object>
        {
            private readonly ShowObjectView parent;
            readonly TreeView<object> _forTree;

            public RegexTreeViewTextFilter(ShowObjectView parent, TreeView<object> forTree)
            {
                this.parent = parent;
                _forTree = forTree ?? throw new ArgumentNullException(nameof(forTree));
            }

            private string text;

            public string Text
            {
                get { return text; }
                set
                {
                    text = value;
                    RefreshTreeView();
                }
            }

            private void RefreshTreeView()
            {
                _forTree.InvalidateLineMap();
                _forTree.SetNeedsDisplay();
            }

            public bool IsMatch(object model)
            {
                if (string.IsNullOrWhiteSpace(Text))
                {
                    return true;
                }

                parent.SetRegexError(string.Empty);

                var modelText = _forTree.AspectGetter(model);
                try
                {
                    return Regex.IsMatch(modelText, text, RegexOptions.IgnoreCase);
                }
                catch (RegexParseException e)
                {
                    parent.SetRegexError(e.Message);
                    return true;
                }
            }
        }
    }
}
