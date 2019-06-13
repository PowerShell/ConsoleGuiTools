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

namespace OutGridView.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly DataSource _dataSource;
        public PsObjectListViewModel PsObjects { get; }

        [Reactive] public PSObject SelectedPsObject { get; set; }

        [Reactive] public string Filter { get; set; }

        public MainWindowViewModel(Database db)
        {
            _dataSource = db.GetDataSource();

            PsObjects = new PsObjectListViewModel(_dataSource);

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                Disposable
                    .Create(() => { /* handle deactivation */ })
                    .DisposeWith(disposables);
                this.HandleActivation();
            });
        }

        private void HandleActivation()
        {

        }

    }
}
