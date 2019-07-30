using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using OutGridView.ViewModels;
using OutGridView.Views;
using OutGridView.Models;
using OutGridView.Services;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

using System.Threading;

//Used to run the module directly from Dotnet
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
            var applicationData = new ApplicationData
            {
                Title = "TestTitle",
                OutputMode = OutputModeOption.Single,
                PassThru = true,
            };
            AvaloniaAppRunner.RunApp(applicationData);

            var passThruObjects = AvaloniaAppRunner.GetPassThruObjects();
        }
    }
}
