// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.Configuration;
using Microsoft.PowerShell.OutGridView.Models;

namespace Microsoft.PowerShell.ConsoleGuiTools;

/// <summary>
///     Provides the Terminal.Gui Window implementation for displaying object hierarchies in a tree view with filtering capabilities.
/// </summary>
internal sealed class ShowObjectTreeWindow : Window, ITreeBuilder<object>
{
    private const string FILTER_LABEL = "_Filter:";

    private readonly TreeView<object>? _tree;
    private Shortcut? _selectedShortcut;
    private StatusBar? _statusBar;
    private readonly ApplicationData _applicationData;

    private View? _filterErrorView;
    private TextField? _filterField;
    private Label? _filterLabel;

    /// <summary>
    ///     Gets a value indicating whether this tree builder supports the CanExpand operation.
    /// </summary>
    public bool SupportsCanExpand => true;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ShowObjectTreeWindow" /> class with the specified application data.
    /// </summary>
    /// <param name="applicationData">The configuration and PSObjects to display.</param>
    public ShowObjectTreeWindow(ApplicationData applicationData)
    {
        _applicationData = applicationData;
        Title = _applicationData.Title ?? "Show-ObjectView";

        SchemeName = SchemeManager.SchemesToSchemeName(Schemes.Base);
        BorderStyle = FrameView.DefaultBorderStyle;

        switch (_applicationData.MinUI)
        {
            case true:
                BorderStyle = LineStyle.None;
                if (!string.IsNullOrEmpty(_applicationData.Filter)) AddFilter();
                break;
            case false:
                Border.Thickness = new Thickness(0, 2, 0, 0);
                AddFilter();
                break;
        }

        // Extract root objects from PSObjects
        var rootObjects = _applicationData.PSObjects?.Select(p =>
        {
            if (p is PSObject pso)
                return pso.BaseObject;
            return p;
        }).ToList() ?? [];

        _tree = new TreeView<object>
        {
            Y = _filterErrorView is not null ? Pos.Bottom(_filterErrorView) : 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            TreeBuilder = this,
            Style = new TreeStyle() { HighlightModelTextOnly = true },
            AspectGetter = AspectGetter!
        };

        var regexFilter = new RegexTreeViewTextFilter(this, _tree!)
        {
            Text = _applicationData.Filter ?? string.Empty
        };
        _tree?.Filter = regexFilter;

        if (rootObjects.Count > 0)
            _tree?.AddObjects(rootObjects);
        else
            _tree?.AddObject("No Objects");

        Add(_tree);
    }

    /// <summary>
    ///     Adds the filter text field and error display to the window.
    /// </summary>
    private void AddFilter()
    {
        _filterLabel = new Label
        {
            Text = FILTER_LABEL
        };

        _filterField = new TextField
        {
            Text = _applicationData.Filter ?? string.Empty,
            X = Pos.Right(_filterLabel) + 1,
            Y = Pos.Top(_filterLabel),
            CanFocus = true,
            Width = Dim.Fill() - 1
        };

        _filterField.KeyBindings.Remove(Key.A.WithCtrl);
        _filterField.KeyBindings.Remove(Key.D.WithCtrl);

        _filterErrorView = new View
        {
            Text = string.Empty,
            X = Pos.Right(_filterLabel) + 1,
            Y = Pos.Top(_filterLabel) + 1,
            Width = Dim.Fill() - _filterLabel.Text.Length,
            Height = Dim.Auto(DimAutoStyle.Text),
            SchemeName = SchemeManager.SchemesToSchemeName(Schemes.Error)
        };

        _filterField.TextChanged += (_, _) =>
        {
            var filterText = _filterField.Text;
            try
            {
                _filterErrorView?.Text = string.Empty;
                _applicationData.Filter = filterText;
                _filterField?.TextChanged += (sender, _) => OnFilterTextChanged(sender, ((RegexTreeViewTextFilter)_tree?.Filter));
            }
            catch (Exception ex)
            {
                _filterErrorView?.Text = ex.Message;
            }
        };

        Add(_filterLabel, _filterField, _filterErrorView);

        _filterField.Text = _applicationData.Filter ?? string.Empty;
        _filterField.InsertionPoint = _filterField.Text.Length;
    }


    /// <summary>
    ///     Adds the status bar with keyboard shortcuts to the window.
    /// </summary>
    private void AddStatusBar()
    {
        if (_tree is { Objects: not null })
        {
            var shortcuts = CreateShortcuts(_tree.Objects.ToList());

            if (_applicationData.Verbose || _applicationData.Debug)
            {
                shortcuts.Add(new Shortcut(Key.Empty, $" v{_applicationData.ModuleVersion}", null));
                var tgFileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(Application))!.Location);
                var tgVersion = tgFileVersionInfo.FileVersion ?? "no version found";
                //if (tgFileVersionInfo is { IsPreRelease: true })
                {
                    var plusIdx = tgFileVersionInfo.ProductVersion?.IndexOf('+') ?? -1;
                    tgVersion = plusIdx >= 0
                        ? tgFileVersionInfo.ProductVersion![..plusIdx]
                        : tgFileVersionInfo.ProductVersion ?? tgVersion;
                }
                shortcuts.Add(new Shortcut(Key.Empty, $"{App?.Driver?.GetName()} v{tgVersion}", null));
            }

            _statusBar = new StatusBar(shortcuts);
        }

        Add(_statusBar);
    }

    #region Event Handlers

    private int _maxHeight;
    private bool _laidOut;

    protected override void OnIsRunningChanged(bool newIsRunning)
    {
        base.OnIsRunningChanged(newIsRunning);
        if (!newIsRunning) return;

        App?.LayoutAndDrawComplete += (_, _) =>
        {
            _maxHeight = !_laidOut ? Frame.Height : Math.Max(_maxHeight, Frame.Height);
            _laidOut = true;
        };

        if (App?.AppModel == AppModel.Inline && Height.Has(out DimFill _) && _tree != null)
        {
            // If starting inline and height is Dim.Fill, change to Dim.Auto to avoid full screen
            Height = Dim.Auto();

            _tree.Height = Dim.Auto(
                minimumContentDim:
                Dim.Func(_ => Math.Max(
                    _tree.GetSize().Height,
                    _maxHeight - (_tree.FrameToScreen().Top + _tree.GetAdornmentsThickness().Vertical + (_statusBar?.Frame.Height ?? 0) + Border.Thickness.Bottom))),
                maximumContentDim:
                Dim.Func(_ =>
                    App?.Driver?.Screen.Height -
                    (_tree.FrameToScreen().Top + _tree.GetAdornmentsThickness().Vertical + (_statusBar?.Frame.Height ?? 0) + Border.Thickness.Bottom) ?? 0));
        }

        // We do this here, because _statusBar requires the Application to be running to
        // access the driver information.
        if (!_applicationData.MinUI) AddStatusBar();

        _tree?.SetFocus();
    }

    private int GetEpxandedRows()
    {
        int count = 0;

        return count;
    }

    /// <summary>
    ///     Handles filter text changes and applies the regex filter.
    /// </summary>
    /// <param name="sender">The text field that triggered the event.</param>
    /// <param name="regexFilter">The regex filter to update.</param>
    private void OnFilterTextChanged(object? sender, RegexTreeViewTextFilter regexFilter)
    {
        if (sender is not TextField textField) return;

        // Test that the regex is valid before applying it
        try
        {
            _ = new Regex(textField.Text, RegexOptions.IgnoreCase);
        }
        catch (RegexParseException ex)
        {
            _filterErrorView?.Text = ex.Message;
            return;
        }

        _filterErrorView?.Text = string.Empty;
        regexFilter.Text = textField.Text;
    }

    /// <summary>
    ///     Handles selection changes in the tree view and updates the status bar.
    /// </summary>
    /// <param name="sender">The tree view that triggered the event.</param>
    /// <param name="e">The selection changed event arguments.</param>
    private void SelectionChanged(object? sender, SelectionChangedEventArgs<object> e)
    {
        var selectedValue = e.NewValue;

        if (selectedValue is CachedMemberResult cmr)
            selectedValue = cmr.Value;

        _selectedShortcut?.Title = selectedValue != null ? selectedValue.GetType().Name : string.Empty;

        _statusBar?.SetNeedsDraw();
    }

    #endregion

    #region Public Methods

    /// <summary>
    ///     Sets the regex error message displayed in the filter error view.
    /// </summary>
    /// <param name="error">The error message to display.</param>
    internal void SetRegexError(string error)
    {
        if (string.Equals(error, _filterErrorView?.Text, StringComparison.Ordinal)) return;
        _filterErrorView?.Text = error;
    }

    #endregion

    #region ITreeBuilder Implementation

    /// <summary>
    ///     Determines whether the specified object can be expanded to show children.
    /// </summary>
    /// <param name="toExpand">The object to check for expansion capability.</param>
    /// <returns><see langword="true" /> if the object can be expanded; otherwise, <see langword="false" />.</returns>
    public bool CanExpand(object toExpand)
    {
        if (toExpand is CachedMemberResult p) return IsBasicType(p.Value);

        return IsBasicType(toExpand);
    }

    /// <summary>
    ///     Gets the child objects for the specified parent object.
    /// </summary>
    /// <param name="forObject">The parent object to get children for.</param>
    /// <returns>An enumerable collection of child objects.</returns>
    public IEnumerable<object> GetChildren(object? forObject)
    {
        while (true)
        {
            if (forObject == null || !CanExpand(forObject)) return [];

            switch (forObject)
            {
                case CachedMemberResult { IsCollection: true } p:
                    return p.Elements ?? Enumerable.Empty<object>();
                case CachedMemberResult p:
                    forObject = p.Value;
                    continue;
                case CachedMemberResultElement e:
                    forObject = e.Value;
                    continue;
            }

            var children = new List<object>();

            foreach (var member in forObject.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public)
                         .OrderBy(m => m.Name))
            {
                if (member is PropertyInfo prop)
                    children.Add(new CachedMemberResult(forObject, prop));

                if (member is FieldInfo field)
                    children.Add(new CachedMemberResult(forObject, field));
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
    }

    #endregion

    #region Helper Methods

    /// <summary>
    ///     Gets the display text for an object in the tree view.
    /// </summary>
    /// <param name="toRender">The object to get display text for.</param>
    /// <returns>The display text for the object.</returns>
    private string? AspectGetter(object? toRender)
    {
        return toRender switch
        {
            Process p => p.ProcessName,
            null => "Null",
            FileSystemInfo fsi when !IsRootObject(fsi) => fsi.Name,
            _ => toRender.ToString()
        };
    }

    /// <summary>
    ///     Determines whether the specified object is a root object in the tree.
    /// </summary>
    /// <param name="o">The object to check.</param>
    /// <returns><see langword="true" /> if the object is a root object; otherwise, <see langword="false" />.</returns>
    private bool IsRootObject(object o) => _tree is { Objects: not null } && _tree.Objects.Contains(o);

    /// <summary>
    ///     Determines whether the specified value is a basic (non-primitive, non-string) type that can be expanded.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true" /> if the value is a basic type; otherwise, <see langword="false" />.</returns>
    private static bool IsBasicType(object? value) =>
        value != null && value is not string && !value.GetType().IsValueType;

    /// <summary>
    ///     Gets additional child objects for special types like DirectoryInfo.
    /// </summary>
    /// <param name="forObject">The object to get extra children for.</param>
    /// <returns>An enumerable collection of additional child objects.</returns>
    private static IEnumerable<object> GetExtraChildren(object forObject)
    {
        if (forObject is DirectoryInfo dir)
            foreach (var c in dir.EnumerateFileSystemInfos())
                yield return c;
    }

    /// <summary>
    ///     Creates the keyboard shortcuts for the status bar.
    /// </summary>
    /// <param name="rootObjects">The root objects being displayed.</param>
    /// <returns>A list of shortcuts for the status bar.</returns>
    private List<Shortcut> CreateShortcuts(List<object> rootObjects)
    {
        var shortcuts = new List<Shortcut>();

        var elementDescription = "objects";
        var types = rootObjects.Select(o => o.GetType()).Distinct().ToArray();
        if (types.Length == 1)
            elementDescription = types[0].Name;

        shortcuts.Add(new Shortcut(Key.Esc, "Close", () => App?.RequestStop()));

        var countShortcut = new Shortcut(Key.Empty, $"{rootObjects.Count} {elementDescription}", null);
        _selectedShortcut = new Shortcut(Key.Empty, string.Empty, null);
        shortcuts.Add(countShortcut);
        shortcuts.Add(_selectedShortcut);

        return shortcuts;
    }

    #endregion
}
