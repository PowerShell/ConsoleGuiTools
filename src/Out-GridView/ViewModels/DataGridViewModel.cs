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
using System.Reflection;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace OutGridView.ViewModels
{
    public class DataGridViewModel : ViewModelBase
    {
        private ReadOnlyObservableCollection<PSObject> _viewObjects;
        public ReadOnlyObservableCollection<PSObject> ViewObjects => _viewObjects;
        public SourceList<Column> Columns { get; } = new SourceList<Column>();
        private ReadOnlyObservableCollection<Column> _columnSelect;
        public ReadOnlyObservableCollection<Column> ColumnSelect => _columnSelect;
        public IList<PSObject> SelectedObjects { [ObservableAsProperty]get; }

        public DataGridViewModel(IObservableList<PSObject> objects, IEnumerable<PropertyInfo> properties)
        {
            Columns.AddRange(properties.Select(x => new Column(x)));

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                Columns.Connect()
                    .AutoRefresh()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Bind(out _columnSelect)
                    .Subscribe();



                objects.Connect()
                    .Bind(out _viewObjects)
                    .Subscribe(Console.Write);
            });
        }


    }
}
