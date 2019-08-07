using System;
using System.Collections.Generic;
using System.Text.Json;

namespace OutGridView.Models
{
    public class ApplicationData
    {
        public string Title { get; set; }
        public OutputModeOption OutputMode { get; set; }
        public bool PassThru { get; set; }
        public DataTable DataTable { get; set; }
    }
}