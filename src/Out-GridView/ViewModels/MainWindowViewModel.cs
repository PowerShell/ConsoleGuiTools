using System;
using System.Collections.Generic;
using System.Text;
using OutGridView.Services;
using OutGridView.ViewModels;
using OutGridView.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Management.Automation;
using ReactiveUI.Fody.Helpers;
using DynamicData;
using System.Linq;
using System.Reactive.Linq;
using DynamicData.Binding;
using System.Reactive;

namespace OutGridView.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly DataTable dataTable;
        [Reactive] public FilterQueryBuilderViewModel FilterQueryBuilder { get; set; }
        [Reactive] public DataGridViewModel DataGridView { get; set; }
        [Reactive] public string SearchText { get; set; } = String.Empty;
        public readonly bool IsPassThruEnabled;
        public readonly string Title;
        public ReactiveCommand<Unit, Unit> PassThruOkCommand { get; }
        public ReactiveCommand<Unit, Unit> PassThruCancelCommand { get; }
        private readonly OutputModeOption outputMode;
        public List<PSObject> OutputObjects { get; set; }

        public MainWindowViewModel(Database db)
        {

            dataTable = db.GetDataTable();

            IsPassThruEnabled = db.PassThru;
            Title = db.Title;
            outputMode = db.OutputMode;

            PassThruOkCommand = ReactiveCommand.Create(OnPassThruOk);
            PassThruCancelCommand = ReactiveCommand.Create(OnPassThruCancel);

            this.WhenActivated((CompositeDisposable disposables) =>
            {

                var observableColumns = new SourceList<DataTableColumn>();

                observableColumns.AddRange(dataTable.DataColumns);

                var observableData = new SourceList<DataTableRow>();

                observableData.AddRange(dataTable.Data);

                FilterQueryBuilder = new FilterQueryBuilderViewModel(observableColumns);

                var filterData = Observable.Merge(this.WhenAnyValue(x => x.SearchText).Select(_ => Unit.Default),
                    FilterQueryBuilder.FiltersByDataColumn.Connect().AutoRefresh().Select(_ => Unit.Default));

                var filterPredicate = filterData.Select(x => FilterBuilder.BuildFilter(SearchText, FilterQueryBuilder.FiltersByDataColumn));

                var filteredObjects = observableData.Connect()
                    .Filter(filterPredicate)
                    .AsObservableList();

                DataGridView = new DataGridViewModel(dataTable.DataColumns, filteredObjects, outputMode);
            });
        }

        public void OnPassThruOk()
        {
            OutputObjects = DataGridView.SelectedRows.Select(x => x.OriginalObject).ToList();

            CloseWindow();
        }

        public void OnPassThruCancel()
        {
            CloseWindow();
        }

        public void CloseWindow()
        {
            App.Current.MainWindow.Close();
        }
    }
}
