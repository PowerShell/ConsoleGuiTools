using System;
using System.Collections.Generic;
using System.Reflection;
using OutGridView.Models;
using ReactiveUI;
using DynamicData;
using DynamicData.ReactiveUI;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using ReactiveUI.Fody.Helpers;

namespace OutGridView.ViewModels
{
    public class FilterQueryBuilderViewModel : ViewModelBase
    {
        public IObservableList<PropertyInfo> PropertyOptions { get; }
        private ReadOnlyObservableCollection<PropertyInfo> _visiblePropertyOptions;
        public ReadOnlyObservableCollection<PropertyInfo> VisiblePropertyOptions => _visiblePropertyOptions;
        [Reactive] public PropertyInfo SelectedAddColumn { get; set; }
        public SourceList<Filter> Filters { get; } = new SourceList<Filter>();
        public IObservableList<FilterGroup> FiltersByPropertyInfo { get; set; }
        private ReadOnlyObservableCollection<FilterGroup> _filtersByPropertyInfoView;
        public ReadOnlyObservableCollection<FilterGroup> FiltersByPropertyInfoView => _filtersByPropertyInfoView;
        public ReactiveCommand<PropertyInfo, Unit> AddFilterCommand { get; }
        public ReactiveCommand<Filter, Unit> RemoveFilterCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; }

        public FilterQueryBuilderViewModel(IObservableList<PropertyInfo> propertyOptions)
        {
            PropertyOptions = propertyOptions;

            AddFilterCommand = ReactiveCommand.Create<PropertyInfo>(AddFilter);
            RemoveFilterCommand = ReactiveCommand.Create<Filter>(RemoveFilter);
            ClearFiltersCommand = ReactiveCommand.Create(ClearFilters);


            this.WhenActivated((CompositeDisposable disposables) =>
            {
                var filterGroups = Filters.Connect()
                    .GroupWithImmutableState(x => x.Property)
                    .Transform(grouping => new FilterGroup(grouping.Key, grouping.Items))
                    .ObserveOn(RxApp.MainThreadScheduler);

                FiltersByPropertyInfo = Filters.Connect()
                    .AutoRefresh()
                    .GroupWithImmutableState(x => x.Property)
                    .Transform(grouping => new FilterGroup(grouping.Key, grouping.Items))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .AsObservableList();

                filterGroups
                    .Bind(out _filtersByPropertyInfoView)
                    .Subscribe();

                var activeProperties = filterGroups
                    .Transform(x => x.Key);

                PropertyOptions.Connect()
                    .Except(activeProperties)
                    .Bind(out _visiblePropertyOptions)
                    .Subscribe();
            });
        }

        private void AddFilter(PropertyInfo property)
        {
            Filters.Add(new Filter(property));
            SelectedAddColumn = null;
        }

        private void RemoveFilter(Filter filter)
        {
            Filters.Remove(filter);
        }

        private void ClearFilters()
        {
            Filters.Clear();
        }
    }
}