// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using OutGridView.Application.Models;
using ReactiveUI;
using DynamicData;
using DynamicData.ReactiveUI;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using ReactiveUI.Fody.Helpers;
using DynamicData.Aggregation;
using OutGridView.Models;
using OutGridView.Application.Services;

namespace OutGridView.Application.ViewModels
{
    public class FilterQueryBuilderViewModel : ViewModelBase
    {
        private SourceList<DataTableColumn> DataColumnOptions = new SourceList<DataTableColumn>();

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
        public ReactiveCommand<Unit, Unit> ShowCodeCommand { get; }
        public Boolean IsColumnSelectVisible { [ObservableAsProperty] get; }

        //Placeholder hack for combo box
        private DataTableColumn placeholderColumn = new DataTableColumn("Add Column Filter", "System.Int32");

        public FilterQueryBuilderViewModel(IObservableList<DataTableColumn> dataColumns)
        {

            DataColumnOptions.Add(placeholderColumn);
            DataColumnOptions.AddRange(dataColumns.Items);

            SelectedAddColumn = placeholderColumn;

            AddFilterCommand = ReactiveCommand.Create<DataTableColumn>(AddFilter);
            RemoveFilterCommand = ReactiveCommand.Create<Filter>(RemoveFilter);
            ClearFiltersCommand = ReactiveCommand.Create(ClearFilters);
            ShowCodeCommand = ReactiveCommand.Create(ShowCode);


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
                    .AutoRefresh()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Bind(out _filtersByDataColumnView)
                    .DisposeMany()
                    .Subscribe();

                var activeDataColumns = Filters.Connect()
                    .Transform(x => x.DataColumn)
                    .DistinctValues(x => x)
                    .DisposeMany()
                    .ObserveOn(RxApp.MainThreadScheduler);

                var dataColumnOptions = DataColumnOptions.Connect()
                    .Except(activeDataColumns)
                    .Publish();

                dataColumnOptions
                    .Bind(out _visibleDataColumnOptions)
                    .Subscribe();

                dataColumnOptions
                    .Count()
                    .Select(x => x > 1)
                    .ToPropertyEx(this, x => x.IsColumnSelectVisible);

                dataColumnOptions.Connect();
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

        private void ShowCode()
        {
            var filterGroupList = _filtersByDataColumnView.ToList();
            var filterString = PowerShellCodeGenerator.GetPowershellForFilterGroups(filterGroupList);

            //TODO: Inject service
            var modalService = new ModalService();

            modalService.ShowCodeModal(filterString);
        }

        private void ClearFilters()
        {
            Filters.Clear();
        }
    }
}
