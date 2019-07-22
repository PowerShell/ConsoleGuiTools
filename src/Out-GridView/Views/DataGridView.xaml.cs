using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OutGridView.ViewModels;
using OutGridView.Models;
using System.Reactive.Disposables;
using ReactiveUI;
using System.Collections.Generic;
using ReactiveUI.Fody.Helpers;
using System.Management.Automation;
using System.Linq;
using DynamicData;
using Avalonia.Data;
using System;

namespace OutGridView.Views
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
                        .BindTo(this, x => x.ViewModel.SelectedObjects)
                        .DisposeWith(disposables);

                    ViewModel.Columns.Connect()
                        .AutoRefresh()
                        .Filter(x => x.IsVisible)
                        .Transform(x => new DataGridTextColumn()
                        {
                            Binding = new Binding("Data[" + x.DataColumn.Index + "]", BindingMode.OneTime),
                            Header = x.DataColumn.Label,
                            CanUserReorder = true,
                            CanUserSort = true,
                        })
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
    }
}