using System;
using Avalonia;
using System.Collections.Generic;
using System.Management.Automation;
using Avalonia.Logging.Serilog;
using OutGridView.ViewModels;
using OutGridView.Views;
using OutGridView.Services;
using System.Threading;

namespace OutGridView
{
    public static class AvaloniaAppRunner
    {
        public static App App;
        public static AppBuilder Builder;
        private static List<PSObject> _objects;
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
        public static void RunApp(List<PSObject> objects)
        {
            _objects = objects;

            var thread = Thread.CurrentThread.GetApartmentState();

            Builder.Start(AppMain, new string[] { });
        }
        private static void AppMain(Application app, string[] args)
        {
            var db = new Database(_objects);
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel(db),
            };

            app.Run(window);
        }
    }
}
