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
using DynamicData.ReactiveUI;

namespace OutGridView.ViewModels
{
    public class FilterQueryBuilderViewModel : ViewModelBase
    {
        private SourceList<DataTableColumn> DataColumnOptions { get; } = new SourceList<DataTableColumn>();

        private ReadOnlyObservableCollection<DataTableColumn> _visibleDataColumnOptions;
        public ReadOnlyObservableCollection<DataTableColumn> VisibleDataColumnOptions => _visibleDataColumnOptions;
        [Reactive] public DataTableColumn SelectedAddColumn { get; set; }
        public SourceList<Filter> Filters { get; } = new SourceList<Filter>();
        public IObservableList<FilterGroup> FiltersByDataColumn { get; set; }
        private ReadOnlyObservableCollection<FilterGroup> _filtersByDataColumnView;
        public ReadOnlyObservableCollection<FilterGroup> FiltersByDataColumnView => _filtersByDataColumnView;
        public ReactiveCommand<DataTableColumn, Unit> AddFilterCommand { get; }
        public ReactiveCommand<Filter, Unit> RemoveFilterCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; }
        public Boolean IsColumnSelectVisible { [ObservableAsProperty] get; } = true;

        //Placeholder hack for combo box
        private DataTableColumn placeholderColumn = new DataTableColumn("Add Filter", -1);

        public FilterQueryBuilderViewModel(IObservableList<DataTableColumn> dataColumnOptions)
        {

            SelectedAddColumn = placeholderColumn;

            DataColumnOptions.Add(placeholderColumn);
            DataColumnOptions.AddRange(dataColumnOptions.Items);

            AddFilterCommand = ReactiveCommand.Create<DataTableColumn>(AddFilter);
            RemoveFilterCommand = ReactiveCommand.Create<Filter>(RemoveFilter);
            ClearFiltersCommand = ReactiveCommand.Create(ClearFilters);


            this.WhenActivated((CompositeDisposable disposables) =>
            {
                var filterGroups = Filters.Connect()
                    .GroupWithImmutableState(x => x.DataColumn)
                    .Transform(grouping => new FilterGroup(grouping.Key, grouping.Items));

                FiltersByDataColumn = Filters.Connect()
                    .AutoRefresh()
                    .GroupWithImmutableState(x => x.DataColumn)
                    .Transform(grouping => new FilterGroup(grouping.Key, grouping.Items))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .DisposeMany()
                    .AsObservableList();

                filterGroups
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Bind(out _filtersByDataColumnView)
                    .DisposeMany()
                    .Subscribe();

                var activeDataColumns = Filters.Connect()
                    .Transform(x => x.DataColumn)
                    .DistinctValues(x => x)
                    .DisposeMany()
                    .ObserveOn(RxApp.MainThreadScheduler);

                DataColumnOptions.Connect()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Except(activeDataColumns)
                    .Bind(out _visibleDataColumnOptions)
                    .Subscribe();

                DataColumnOptions.Connect()
                    .Count()
                    .Select(x => x > 1)
                    .ToPropertyEx(this, x => x.IsColumnSelectVisible);





            });
        }

        private void AddFilter(DataTableColumn dataColumn)
        {
            if (dataColumn == placeholderColumn) return;

            Filters.Add(new Filter(dataColumn));
            SelectedAddColumn = placeholderColumn;
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