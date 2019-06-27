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
using Avalonia.Controls;
using DynamicData;
using Avalonia;
using OutGridView.Views;
using DynamicData.ReactiveUI;
using System.Reactive.Linq;


namespace OutGridView.ViewModels
{
    public class DataGridViewModel : ViewModelBase
    {
        private ReadOnlyObservableCollection<PSObject> _viewObjects;
        public ReadOnlyObservableCollection<PSObject> ViewObjects => _viewObjects;
        public SourceList<string> ActiveColumns { get; } = new SourceList<string>();
        public IList<PSObject> SelectedObjects { [ObservableAsProperty]get; }

        public DataGridViewModel(IObservableList<PSObject> objects, IEnumerable<string> properties)
        {
            ActiveColumns.AddRange(properties);

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                objects.Connect()
                    .Bind(out _viewObjects)
                    .Subscribe();
            });
        }

        public void OpenColumn()
        {
            var dialog = new ColumnSelectDialog(ActiveColumns);
            dialog.ShowDialog(Application.Current.MainWindow);
        }
    }
}
