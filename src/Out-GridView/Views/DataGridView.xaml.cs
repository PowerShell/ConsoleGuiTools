using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OutGridView.ViewModels;
using System.Reactive.Disposables;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections;
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
                    DataGridTable.WhenAnyValue(x => x.SelectedItems, x => x.Cast<PSObject>().ToList())
                       .ToPropertyEx(ViewModel, x => x.SelectedObjects);

                    ViewModel.ActiveColumns.Connect()
                        .Transform(x => new DataGridTextColumn()
                        {
                            Binding = new Binding("BaseObject." + x),
                            Header = x,
                            CanUserReorder = true,
                            CanUserSort = true,
                        })
                        .Bind(out var columns)
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