// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using OutGridView.Application.Services;
using ReactiveUI;
using System.Reactive.Disposables;
using ReactiveUI.Fody.Helpers;
using DynamicData;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using Avalonia.Controls;
using OutGridView.Models;
using System.Collections.Generic;

namespace OutGridView.Application.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly DataTable dataTable;
        [Reactive] public FilterQueryBuilderViewModel FilterQueryBuilder { get; set; }
        [Reactive] public DataGridViewModel DataGridView { get; set; }
        [Reactive] public string SearchText { get; set; } = String.Empty;
        public bool IsPassThruEnabled { get; }
        public string Title { get; }
        public ReactiveCommand<Window, Unit> PassThruOkCommand { get; }
        public ReactiveCommand<Window, Unit> PassThruCancelCommand { get; }
        private readonly OutputModeOption outputMode;
        public List<int> OutputObjectIndexes { get; set; } = new List<int>();
        public MainWindowViewModel(ApplicationData applicationData)
        {
            IsPassThruEnabled = applicationData.PassThru;
            Title = applicationData.Title;
            outputMode = applicationData.OutputMode;
            dataTable = applicationData.DataTable;

            PassThruOkCommand = ReactiveCommand.Create<Window>(OnPassThruOk);
            PassThruCancelCommand = ReactiveCommand.Create<Window>(OnPassThruCancel);


            var observableColumns = new SourceList<DataTableColumn>();

            observableColumns.AddRange(dataTable.DataColumns);

            var observableData = new SourceList<DataTableRow>();

            observableData.AddRange(dataTable.Data);
            this.WhenActivated((CompositeDisposable disposables) =>
            {



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

        public void OnPassThruOk(Window window)
        {
            OutputObjectIndexes = DataGridView.SelectedRows.Select(x => x.OriginalObjectIndex).ToList();

            CloseProgam(window);
        }

        public void OnPassThruCancel(Window window)
        {
            CloseProgam(window);
        }

        public void CloseProgam(Window window)
        {
            window.Close();
        }
    }
}
