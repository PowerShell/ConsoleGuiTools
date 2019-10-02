// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using GraphicalTools.Application.ViewModels;
using GraphicalTools.Application.Views;
using GraphicalTools.Application.Services;
using System.Collections.Generic;
using GraphicalTools.Models;
using System.Text;
using System.Diagnostics;

//Used to run the module directly from Dotnet
namespace GraphicalTools.Application
{
    public class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            var base64ApplicationData = Console.ReadLine();

            var applicationData = Serializers.ObjectFromJson<ApplicationData>(base64ApplicationData);

            AvaloniaAppRunner.RunApp(applicationData);

            var passThruIndexes = AvaloniaAppRunner.GetPassThruIndexes();

            Console.WriteLine(Serializers.ObjectToJson(passThruIndexes));

        }
    }
}
