// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using OutGridView.Application.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using Avalonia.Controls;
using DynamicData;
using Avalonia;
using OutGridView.Application.Views;
using DynamicData.ReactiveUI;
using System.Reactive.Linq;
using System.Reflection;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using OutGridView.Models;


namespace OutGridView.Application.ViewModels
{
    public class DataGridViewModel : ViewModelBase
    {
        private ReadOnlyObservableCollection<DataTableRow> _viewObjects;
        public ReadOnlyObservableCollection<DataTableRow> ViewObjects => _viewObjects;

        public SourceList<Column> Columns { get; } = new SourceList<Column>();
        private ReadOnlyObservableCollection<Column> _columnSelect;
        public ReadOnlyObservableCollection<Column> ColumnSelect => _columnSelect;
        public DataGridSelectionMode SelectionMode { get; set; }
        public List<DataTableRow> SelectedRows { get; set; }
        public DataGridViewModel(List<DataTableColumn> dataColumns, IObservableList<DataTableRow> data, OutputModeOption outputMode)
        {
            var columns = dataColumns.Select(x => new Column(x));

            Columns.AddRange(columns);

            SelectionMode = OutputModeToSelectionMode(outputMode);

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                Columns.Connect()
                        .AutoRefresh()
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Bind(out _columnSelect)
                        .Subscribe();

                data.Connect()
                    .Bind(out _viewObjects)
                    .Subscribe();
            });
        }

        public DataGridSelectionMode OutputModeToSelectionMode(OutputModeOption outputModeOption)
        {
            switch (outputModeOption)
            {
                case OutputModeOption.None:
                    return DataGridSelectionMode.Extended;
                case OutputModeOption.Single:
                    return DataGridSelectionMode.Single;
                case OutputModeOption.Multiple:
                    return DataGridSelectionMode.Extended;
                default:
                    return DataGridSelectionMode.Extended;
            }
        }
    }
}
