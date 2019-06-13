using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OutGridView.ViewModels;
using System.Reactive.Disposables;
using ReactiveUI;


namespace OutGridView.Views
{
    public partial class CriteriaPanelView : ReactiveUserControl<CriteriaPanelViewModel>
    {
        public CriteriaPanelView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated((CompositeDisposable disposables) =>
              {

              });
            AvaloniaXamlLoader.Load(this);


        }
    }
}