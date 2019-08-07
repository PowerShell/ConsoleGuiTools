using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Reflection;
using OutGridView.Models;
using System.Collections.Generic;

namespace OutGridView.Cmdlet
{
    class AvaloniaProcessBridge
    {
        private static Process _process;

        public static List<int> SelectedIndexes { get; }
        public static void Start(ApplicationData applicationData)
        {
            _process = new Process();
            _process.StartInfo.FileName = GetOutgridViewApplicationLocation();


            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.UseShellExecute = false;



            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;

            _process.OutputDataReceived += (sender, data) =>
            {
                if (!string.IsNullOrWhiteSpace(data.Data))
                {
                    var selectedIndexes = Serializers.ObjectFromJson<List<int>>(data.Data);
                }
            };

            _process.ErrorDataReceived += (sender, data) =>
            {
                Console.WriteLine(data.Data);
            };

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();

            var serializedData = Serializers.ObjectToJson(applicationData);

            _process.StandardInput.WriteLine(serializedData);

        }
        public static void WaitForExit()
        {
            _process.WaitForExit();
        }

        public static void CloseProcess()
        {
            _process.Close();
        }

        public static string GetOutgridViewApplicationLocation()
        {
            return @"C:\Users\t-jozeid\Documents\Code\GraphicalTools\src\Out-GridView\module\OutGridView\bin\Application\win10-x64\OutGridViewApplication.exe";
        }
    }
}
