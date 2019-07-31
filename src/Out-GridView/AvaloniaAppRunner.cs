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
using System.Linq;
using ReactiveUI;
using Avalonia.Threading;

namespace OutGridView
{
    public static class AvaloniaAppRunner
    {
        public static App App;
        public static AppBuilder Builder;
        private static ApplicationData _applicationData;
        private static CancellationTokenSource _source;
        static AvaloniaAppRunner()
        {
            new CustomAssemblyLoadContext().LoadNativeLibraries();
            new CustomAssemblyLoadContext().LoadLibs();
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
        private static void AppMain(Application app)
        {
            var db = new Database(_applicationData);
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel(db),
            };

            _source = new CancellationTokenSource();

            window.Show();
            window.Closing += Window_Closing;

            App.MainWindow = window;

            App.Run(_source.Token);

            _source.Dispose();
        }


        private static void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _source.Cancel();
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
