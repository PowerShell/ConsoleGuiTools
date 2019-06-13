using System;
using System.Collections.Generic;
using OutGridView.ViewModels;
using OutGridView.Models;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using ReactiveUI;
using Avalonia.Controls;
using System.Management.Automation;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using DynamicData.Aggregation;
using DynamicData.Binding;
using DynamicData;
using System.Reflection;


namespace OutGridView.ViewModels
{
    public class CriteriaPanelViewModel : ViewModelBase
    {

        private readonly IEnumerable<PropertyInfo> properties;

        public ReactiveCommand<Unit, Unit> CreateFiltersCommand { get; }
        public ReactiveCommand<Unit, Unit> ClosePanelCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearCriteriaFiltersCommand { get; }
        public ReactiveCommand<CriteriaFilter, Unit> RemoveCriteriaFilterCommand { get; }

        public SourceList<CriteriaCheckbox> CriteriaCheckboxSourceList { get; set; }
        public IObservableCollection<CriteriaCheckbox> CriteriaCheckboxes { get; } = new ObservableCollectionExtended<CriteriaCheckbox>();
        public SourceList<CriteriaFilter> CriteriaFilterSourceList { get; set; } = new SourceList<CriteriaFilter>();

        private ReadOnlyObservableCollection<CriteriaFilter> _criteriaFilterViewList = new ReadOnlyObservableCollection<CriteriaFilter>(new ObservableCollection<CriteriaFilter>());

        public IEnumerable<CriteriaFilterType> CriteriaFilterTypes { get; } = Enum.GetValues(typeof(CriteriaFilterType)).Cast<CriteriaFilterType>();

        public ReadOnlyObservableCollection<CriteriaFilter> CriteriaFilterViewList => _criteriaFilterViewList;
        [Reactive] public bool IsPanelOpen { get; set; }
        public extern bool IsClearCriteriaVisible { [ObservableAsProperty]get; }
        public CriteriaPanelViewModel(IEnumerable<PropertyInfo> _properties)
        {
            properties = _properties;

            CriteriaCheckboxSourceList = new SourceList<CriteriaCheckbox>();

            CreateCheckboxes();

            CreateFiltersCommand = ReactiveCommand.Create(CreateFilters);
            ClosePanelCommand = ReactiveCommand.Create(CloseCriteriaPanel);
            ClearCriteriaFiltersCommand = ReactiveCommand.Create(ClearCriteriaFilters);
            RemoveCriteriaFilterCommand = ReactiveCommand.Create<CriteriaFilter>(RemoveCriteriaFilter);

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                this.HandleActivation();
            });

        }

        private void CreateCheckboxes()
        {
            CriteriaCheckboxSourceList.InsertRange(properties.Select(x => new CriteriaCheckbox(x)), 0);
        }
        private void ResetCheckboxes()
        {
            CriteriaCheckboxSourceList.Clear();
            CreateCheckboxes();
        }

        void ClearCriteriaFilters()
        {
            CriteriaFilterSourceList.Clear();
        }

        void RemoveCriteriaFilter(CriteriaFilter selectedFilter)
        {
            CriteriaFilterSourceList.Remove(selectedFilter);
        }

        void CloseCriteriaPanel()
        {
            ResetCheckboxes();
            IsPanelOpen = false;
        }

        void CreateFilters()
        {
            var newCriteriaFilters = CriteriaCheckboxSourceList
                .Items
                .Where(x => x.IsChecked)
                .Select(x => new CriteriaFilter(x.Property));

            CriteriaFilterSourceList.AddRange(newCriteriaFilters);

            CloseCriteriaPanel();
        }

        private void HandleActivation()
        {
            var criteriaFiltersObservable = CriteriaFilterSourceList.Connect();

            criteriaFiltersObservable
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _criteriaFilterViewList)
                .Subscribe();

            criteriaFiltersObservable
                .Count()
                .Select(x => x > 0)
                .ToPropertyEx(this, x => x.IsClearCriteriaVisible);

            CriteriaCheckboxSourceList.Connect().Bind(CriteriaCheckboxes).Subscribe();

        }
    }
}
