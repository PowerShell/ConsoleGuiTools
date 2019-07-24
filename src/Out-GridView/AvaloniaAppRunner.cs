using System;
using Avalonia;
using System.Collections.Generic;
using System.Management.Automation;
using Avalonia.Logging.Serilog;
using OutGridView.ViewModels;
using OutGridView.Views;
using OutGridView.Services;
using System.Threading;
using OutGridView.Models;

namespace OutGridView
{
    public static class AvaloniaAppRunner
    {
        public static App App;
        public static AppBuilder Builder;
        private static ApplicationData _applicationData;
        static AvaloniaAppRunner()
        {
            new CustomAssemblyLoadContext().LoadNativeLibraries();
            new CustomAssemblyLoadContext().LoadLibs();
            App = new App();
            Thread.CurrentThread.TrySetApartmentState(ApartmentState.STA);
            Builder = BuildAvaloniaApp();
        }

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure(App)
               .UsePlatformDetect()
               .UseDataGrid()
               .LogToDebug()
               .UseReactiveUI();
        public static void RunApp(ApplicationData applicationData)
        {
            _applicationData = applicationData;

            var thread = Thread.CurrentThread.GetApartmentState();

            Builder.Start(AppMain, new string[] { });

        }
        private static void AppMain(Application app, string[] args)
        {
            var db = new Database(_applicationData);
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel(db),
            };

            app.Run(window);
        }
        public static void CloseWindow()
        {
            App.Current.MainWindow.Close();
        }

        public static List<PSObject> GetPassThruObjects()
        {
            var mainWindowContext = App.MainWindow.DataContext as MainWindowViewModel;

            return mainWindowContext.OutputObjects;
        }
    }
}
