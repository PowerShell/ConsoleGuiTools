using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using ReactiveUI;
using System.Management.Automation;
using System.Linq;
using OutGridView.Models;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using OutGridView.Services;
using DynamicData;


namespace OutGridView.ViewModels
{
    public class PsObjectListViewModel : ViewModelBase
    {
        private readonly DataSource _dataSource;
        public IEnumerable<object> ViewItems { [ObservableAsProperty]get; set; }
        [Reactive] public object SelectedObject { get; set; }
        [Reactive] public string Filter { get; set; } = String.Empty;
        [Reactive] private Func<PSObject, bool> FilterPredicate { get; set; }
        public CriteriaPanelViewModel Criteria { get; }
        public PsObjectListViewModel(DataSource dataSource)
        {
            _dataSource = dataSource;

            Criteria = new CriteriaPanelViewModel(_dataSource.PSObjectProperties);


            this.WhenActivated((CompositeDisposable disposables) =>
            {
                this.WhenAnyValue(x => x.Filter)
                  .Subscribe(x =>
                  {
                      FilterPredicate = FilterBuilder.BuildFilter(x, _dataSource.PSObjectProperties, Criteria.CriteriaFilterSourceList.Items);
                  });

                Criteria.CriteriaFilterSourceList.Connect()
                   .AutoRefresh(x => x.Value)
                   .ToCollection()
                   .Subscribe(x =>
                   {
                       FilterPredicate = FilterBuilder.BuildFilter(Filter, _dataSource.PSObjectProperties, Criteria.CriteriaFilterSourceList.Items);
                   });

                this.WhenAnyValue(x => x.FilterPredicate, x => x._dataSource.PSObjects, (x, y) => y.Items.Where(FilterPredicate).Select(ps => ps.BaseObject))
                    .ToPropertyEx(this, x => x.ViewItems);

                Disposable
                    .Create(() => { /* handle deactivation */ })
                    .DisposeWith(disposables);

            });

        }


    }
}