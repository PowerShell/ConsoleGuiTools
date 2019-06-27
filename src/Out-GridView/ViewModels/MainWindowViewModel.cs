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
        private readonly DataSource _dataSource;
        [Reactive] public FilterQueryBuilderViewModel FilterQueryBuilder { get; set; }
        [Reactive] public DataGridViewModel DataGridView { get; set; }
        [Reactive] public string SearchText { get; set; } = String.Empty;
        public MainWindowViewModel(Database db)
        {
            _dataSource = db.GetDataSource();

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                var properties = _dataSource.PSObjectProperties;

                FilterQueryBuilder = new FilterQueryBuilderViewModel(properties);

                var filterData = Observable.Merge(this.WhenAnyValue(x => x.SearchText).Select(_ => Unit.Default),
                    FilterQueryBuilder.FiltersByPropertyInfo.Connect().AutoRefresh().Select(_ => Unit.Default));

                var filterPredicate = filterData.Select(x => FilterBuilder.BuildFilter(SearchText, properties.Items, FilterQueryBuilder.FiltersByPropertyInfo));

                var filteredObjects = _dataSource.PSObjects.Connect()
                    .Filter(filterPredicate)
                    .AsObservableList();

                var stringProperties = properties.Items.Select(x => x.Name);

                DataGridView = new DataGridViewModel(filteredObjects, stringProperties);

            });
        }
    }
}
