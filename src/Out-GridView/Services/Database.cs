using System.Collections.Generic;
using System.Management.Automation;
using OutGridView.Models;
using System.Linq;
using Microsoft.PowerShell.Commands.Internal.Format;

namespace OutGridView.Services
{
    public class Database
    {

        private List<PSObject> _objects { get; set; } = new List<PSObject>();

        public Database() { }
        public Database(List<PSObject> objects)
        {
            _objects = objects;
        }

        private List<PSObject> GetItems()
        {
            _objects = PowerShell.Create().AddCommand("Get-Process").Invoke<PSObject>().Take(100).ToList();

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