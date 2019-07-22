using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using OutGridView.ViewModels;
using OutGridView.Views;
using OutGridView.Services;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

using System.Threading;

namespace OutGridView
{
    public class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().Start(AppMain, args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseDataGrid()
                .LogToDebug()
                .UseReactiveUI();

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Application app, string[] args)
        {

            var db = new Database();
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel(db),
            };

            app.Run(window);
        }
    }
}
