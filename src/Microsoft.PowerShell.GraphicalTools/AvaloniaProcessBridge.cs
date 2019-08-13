// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.InteropServices;
using OutGridView.Models;

namespace OutGridView.Cmdlet
{
    class AvaloniaProcessBridge
    {
        private static Process _process;

        public static List<int> SelectedIndexes { get; set; }
        public static void Start(ApplicationData applicationData)
        {
            SelectedIndexes = new List<int>();

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
                    SelectedIndexes = Serializers.ObjectFromJson<List<int>>(data.Data);
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
        public static bool IsClosed()
        {
            if (_process == null || _process.HasExited)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetOutgridViewApplicationLocation()
        {
            string osRid;
            string executableName;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                osRid = "win-x64";
                executableName = "OutGridView.Gui.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                osRid = "osx-x64";
                executableName = "OutGridView.Gui";
            }
            else
            {
                osRid = "linux-x64";
                executableName = "OutGridView.Gui";
            }

            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "OutGridView.Gui", osRid, executableName);
        }
    }
}
