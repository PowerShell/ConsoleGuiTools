using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OutGridView.Models
{
    public class DataTable
    {
        public ObservableCollection<DataTableRow> Data;
        public List<DataTableColumn> DataColumns;
        public DataTable(List<DataTableColumn> columns, List<DataTableRow> data)
        {
            DataColumns = columns;

            Data = new ObservableCollection<DataTableRow>(data);
        }
    }
}