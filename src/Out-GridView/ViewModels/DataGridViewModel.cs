using System;
using System.Collections.Generic;
using OutGridView.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Management.Automation;
using ReactiveUI.Fody.Helpers;
using Avalonia.Controls;
using DynamicData;
using Avalonia;
using OutGridView.Views;
using DynamicData.ReactiveUI;
using System.Reactive.Linq;
using System.Reflection;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace OutGridView.ViewModels
{
    public class DataGridViewModel : ViewModelBase
    {
        private ReadOnlyObservableCollection<DataTableRow> _viewObjects;
        public ReadOnlyObservableCollection<DataTableRow> ViewObjects => _viewObjects;

        public SourceList<Column> Columns { get; } = new SourceList<Column>();
        private ReadOnlyObservableCollection<Column> _columnSelect;
        public ReadOnlyObservableCollection<Column> ColumnSelect => _columnSelect;
        public List<DataTableRow> SelectedObjects { get; set; }
        public DataGridViewModel(List<DataTableColumn> dataColumns, IObservableList<DataTableRow> data)
        {
            var columns = dataColumns.Select(x => new Column(x));

            Columns.AddRange(columns);

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                Columns.Connect()
                        .AutoRefresh()
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Bind(out _columnSelect)
                        .Subscribe();

                data.Connect()
                    .Bind(out _viewObjects)
                    .Subscribe(Console.Write);
            });
        }
    }

}
