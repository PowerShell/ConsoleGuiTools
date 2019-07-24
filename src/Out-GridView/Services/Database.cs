using System.Collections.Generic;
using System.Management.Automation;
using OutGridView.Models;
using System.Linq;
using Microsoft.PowerShell.Commands.Internal.Format;

namespace OutGridView.Services
{
    public class Database
    {

        private List<PSObject> _objects;
        public string Title { get; set; }
        public bool PassThru { get; set; }
        public OutputModeOption OutputMode { get; set; }
        public Database() { }
        public Database(ApplicationData applicationData)
        {
            _objects = applicationData.Objects;
            Title = applicationData.Title;
            PassThru = applicationData.PassThru;
            OutputMode = applicationData.OutputMode;
        }

        private List<PSObject> GetItems()
        {
            // _objects = PowerShell.Create().AddScript("Get-Process").Invoke<PSObject>().Take(100).ToList();

            return _objects;
        }

        public DataTable GetDataTable()
        {
            var items = GetItems();

            var TG = new TypeGetter(PowerShell.Create());

            FormatViewDefinition fvd = TG.GetFormatViewDefinitonForObject(items.First());

            var dataTable = TypeGetter.CastObjectsToTableView(items, fvd);

            return dataTable;
        }
    };
}