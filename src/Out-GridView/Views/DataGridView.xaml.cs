using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OutGridView.ViewModels;
using System.Reactive.Disposables;
using ReactiveUI;
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
                    DataGridTable.WhenAnyValue(x => x.SelectedItems, x => x.Cast<PSObject>().ToList())
                       .ToPropertyEx(ViewModel, x => x.SelectedObjects);

                    ViewModel.Columns.Connect()
                        .AutoRefresh()
                        .Filter(x => x.IsVisible)
                        .Transform(x => new DataGridTextColumn()
                        {
                            Binding = new Binding("BaseObject." + x.Property.Name),
                            SortMemberPath = "BaseObject." + x.Property.Name,
                            Header = x.Property.Name,
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