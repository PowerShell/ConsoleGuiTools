// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Avalonia;
using System.Collections.Generic;
using Avalonia.Logging.Serilog;
using OutGridView.Application.ViewModels;
using OutGridView.Application.Views;
using OutGridView.Application.Services;
using System.Threading;
using OutGridView.Application.Models;
using System.Linq;
using ReactiveUI;
using Avalonia.Threading;
using Avalonia.Controls;
using OutGridView.Models;

namespace OutGridView.Application
{
    public static class AvaloniaAppRunner
    {
        public static App App;
        public static AppBuilder Builder;
        private static ApplicationData _applicationData;
        private static Window _mainWindow;
        private static CancellationTokenSource _source;
        static AvaloniaAppRunner()
        {
            App = new App();
            Builder = BuildAvaloniaApp();
        }

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure(App)
               .UseReactiveUI()
               .UsePlatformDetect()
               .UseDataGrid()
               .LogToDebug()
               .SetupWithoutStarting();
        public static void RunApp(ApplicationData applicationData)
        {
            _applicationData = applicationData;
            AppMain(App);
        }
        private static void AppMain(Avalonia.Application app)
        {

            _mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(_applicationData),
            };

            _source = new CancellationTokenSource();

            _mainWindow.Show();
            _mainWindow.Closing += Window_Closing;

            App.MainWindow = _mainWindow;

            App.Run(_source.Token);

            _source.Dispose();

        }
        private static void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _source.Cancel();
        }

        public static void CloseProgram()
        {
            _mainWindow.Close();
        }

        public static List<int> GetPassThruIndexes()
        {
            var mainWindowDataContext = _mainWindow.DataContext as MainWindowViewModel;
            return mainWindowDataContext.OutputObjectIndexes;
        }
    }
}
