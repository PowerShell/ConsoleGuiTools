// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace OutGridView.Models
{
    public class ApplicationData
    {
        public string Title { get; set; }
        public OutputModeOption OutputMode { get; set; }
        public bool PassThru { get; set; }
        public string Filter { get; set; }
        public bool MinUI { get; set; }
        public DataTable DataTable { get; set; }

        public bool UseSystemConsole { get; set; }
        public bool Verbose { get; set; }
        public bool Debug { get; set; }

        public string ModuleVersion { get; set; }
    }
}
