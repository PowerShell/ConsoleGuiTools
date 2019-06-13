using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Disposables;
using ReactiveUI;

namespace OutGridView.ViewModels
{
    public partial class ViewModelBase : ReactiveObject, ISupportsActivation
    {
        public ViewModelActivator Activator { get; }

        public ViewModelBase()
        {
            Activator = new ViewModelActivator();
            this.WhenActivated((CompositeDisposable disposables) =>
                    {
                        System.Console.WriteLine("View Model Base Actiavted");
                    });
        }
    }
}
