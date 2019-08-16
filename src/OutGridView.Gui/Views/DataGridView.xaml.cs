// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OutGridView.Application.ViewModels;
using OutGridView.Application.Models;
using System.Reactive.Disposables;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using OutGridView.Application.Converters;
using DynamicData;
using Avalonia.Data;
using OutGridView.Models;
using System;

namespace OutGridView.Application.Views
{
    public class DataGridView : ReactiveUserControl<DataGridViewModel>
    {

        public DataGrid DataGridTable => this.FindControl<DataGrid>("DataGridTable");

        public DataGridView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated((CompositeDisposable disposables) =>
                {
                    DataGridTable.WhenAnyValue(x => x.SelectedItem, x => x.SelectedItems, (x, y) => y.OfType<DataTableRow>().ToList())
                        .BindTo(this, x => x.ViewModel.SelectedRows)
                        .DisposeWith(disposables);

                    //Bind the data columns directly on the DataGrid
                    ViewModel.Columns.Connect()
                        .AutoRefresh()
                        .Filter(x => x.IsVisible)
                        .Transform(ColumnToDataGridTextColumn)
                        .Bind(out var columns)
                        .DisposeMany()
                        .Subscribe(x =>
                        {
                            DataGridTable.Columns.Clear(); //TODO incremental?
                            DataGridTable.Columns.AddRange(columns);
                        });
                });

            AvaloniaXamlLoader.Load(this);
        }
        private DataGridTextColumn ColumnToDataGridTextColumn(Column column)
        {
            var binding = new Binding
            {
                Path = "Values[" + column.DataColumn.ToString() + "]",
                Mode = BindingMode.OneTime
            };

            binding.Converter = new IValueToStringConverter();

            return new DataGridTextColumn()
            {
                Binding = binding,
                Header = column.DataColumn.Label,
                CanUserReorder = true,
                CanUserSort = true,
            };
        }
    }

}
