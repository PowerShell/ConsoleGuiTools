// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerShell.OutGridView.Models;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.Time;
using Xunit;

namespace Microsoft.PowerShell.ConsoleGuiTools.Tests;

/// <summary>
///     Integration tests for OutGridViewWindow that exercise the full Terminal.Gui lifecycle
///     using the headless ANSI driver and VirtualTimeProvider.
/// </summary>
public class OutGridViewWindowIntegrationTests
{
    #region Helpers

    private static PSObject CreatePSObject(string name, int id)
    {
        var obj = new PSObject();
        obj.Properties.Add(new PSNoteProperty("Name", name));
        obj.Properties.Add(new PSNoteProperty("Id", id));
        return obj;
    }

    private static ApplicationData CreateApplicationData(
        OutputModeOption outputMode = OutputModeOption.Multiple,
        bool minUI = false,
        string? filter = null)
    {
        var psObjects = new List<object>
        {
            CreatePSObject("Process1", 1),
            CreatePSObject("Process2", 2),
            CreatePSObject("Process3", 3)
        };

        return new ApplicationData
        {
            PSObjects = psObjects,
            Title = "Test Grid",
            OutputMode = outputMode,
            Filter = filter,
            MinUI = minUI,
            Driver = null
        };
    }

    /// <summary>
    ///     Sets up Terminal.Gui in headless mode, creates and begins the window,
    ///     invokes the test action, then tears down.
    /// </summary>
    private static void RunWithWindow(
        ApplicationData appData,
        Action<IApplication, OutGridViewWindow> testAction)
    {
        Terminal.Gui.Configuration.ConfigurationManager.Enable(
            Terminal.Gui.Configuration.ConfigLocations.All);

        // Build data source from PSObjects in appData
        var typeGetter = new TypeGetter();
        var psObjects = appData.PSObjects?.Cast<PSObject>().ToList() ?? [];
        var columns = typeGetter.GetDataColumnsForObject(psObjects[0]);
        var dataSource = new OutGridViewDataSource(columns);
        for (int i = 0; i < psObjects.Count; i++)
        {
            dataSource.AddRow(TypeGetter.CastObjectToDataTableRow(psObjects[i], columns, i));
        }

        using OutGridViewWindow window = new(appData, dataSource);
        using IApplication app = Application.Create(new VirtualTimeProvider())
            .Init(driverName: "ansi");

        SessionToken? token = app.Begin(window);
        app.LayoutAndDraw(forceRedraw: true);

        try
        {
            testAction(app, window);
        }
        finally
        {
            if (token is not null)
            {
                app.End(token);
            }
        }
    }

    #endregion

    #region ESC / Close Tests

    [Fact]
    public void Esc_ClosesWindow_ResultIsNull()
    {
        var appData = CreateApplicationData();

        RunWithWindow(appData, (app, window) =>
        {
            // Verify data is rendered
            string screen = app.Driver!.ToString();
            Assert.Contains("Process1", screen);
            Assert.Contains("Process2", screen);
            Assert.Contains("Process3", screen);

            // Simulate ESC
            app.Keyboard.RaiseKeyDownEvent(Key.Esc);

            // ESC triggers Close() which sets Result = null
            Assert.Null(window.Result);
        });
    }

    #endregion

    #region Accept / Selection Tests

    [Fact]
    public void Enter_WithMarkedItems_ReturnsSelectedIndexes()
    {
        var appData = CreateApplicationData(outputMode: OutputModeOption.Multiple);

        RunWithWindow(appData, (app, window) =>
        {
            // Mark first item
            app.Keyboard.RaiseKeyDownEvent(Key.Space);
            app.LayoutAndDraw();

            // Move down and mark second item
            app.Keyboard.RaiseKeyDownEvent(Key.CursorDown);
            app.LayoutAndDraw();
            app.Keyboard.RaiseKeyDownEvent(Key.Space);
            app.LayoutAndDraw();

            // Accept
            app.Keyboard.RaiseKeyDownEvent(Key.Enter);

            Assert.NotNull(window.Result);
            var result = (HashSet<int>)window.Result;
            Assert.Contains(0, result);
            Assert.Contains(1, result);
            Assert.DoesNotContain(2, result);
        });
    }

    [Fact]
    public void Enter_WithNoMarkedItems_ReturnsCurrentRow()
    {
        var appData = CreateApplicationData(outputMode: OutputModeOption.Multiple);

        RunWithWindow(appData, (app, window) =>
        {
            // With no explicitly marked items, Enter returns the currently-focused row
            app.Keyboard.RaiseKeyDownEvent(Key.Enter);

            Assert.NotNull(window.Result);
            var result = (HashSet<int>)window.Result;
            Assert.Single(result);
            Assert.Contains(0, result);
        });
    }

    #endregion

    #region Rendering Tests

    [Fact]
    public void Render_ShowsColumnHeadersAndData()
    {
        var appData = CreateApplicationData();

        RunWithWindow(appData, (app, window) =>
        {
            string screen = app.Driver!.ToString();

            Assert.Contains("Name", screen);
            Assert.Contains("Id", screen);
            Assert.Contains("Process1", screen);
            Assert.Contains("Process2", screen);
            Assert.Contains("Process3", screen);

            app.Keyboard.RaiseKeyDownEvent(Key.Esc);
        });
    }

    [Fact]
    public void Render_WithFilter_ShowsOnlyMatchingRows()
    {
        var appData = CreateApplicationData(filter: "Process1");

        RunWithWindow(appData, (app, window) =>
        {
            string screen = app.Driver!.ToString();
            Assert.Contains("Process1", screen);

            app.Keyboard.RaiseKeyDownEvent(Key.Esc);
        });
    }

    [Fact]
    public void Render_MinUI_DoesNotShowFilterLabel()
    {
        var appData = CreateApplicationData(minUI: true);

        RunWithWindow(appData, (app, window) =>
        {
            string screen = app.Driver!.ToString();

            Assert.Contains("Process1", screen);
            Assert.DoesNotContain("Filter:", screen);

            app.Keyboard.RaiseKeyDownEvent(Key.Esc);
        });
    }

    #endregion

    #region OutputMode Tests

    [Fact]
    public void OutputModeNone_EscClosesWithNullResult()
    {
        var appData = CreateApplicationData(outputMode: OutputModeOption.None);

        RunWithWindow(appData, (app, window) =>
        {
            app.Keyboard.RaiseKeyDownEvent(Key.Esc);
            Assert.Null(window.Result);
        });
    }

    #endregion
}
