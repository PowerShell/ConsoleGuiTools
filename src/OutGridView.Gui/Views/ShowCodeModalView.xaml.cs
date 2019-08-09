using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OutGridView.Application.ViewModels;
using System.Reactive.Disposables;
using ReactiveUI;


namespace OutGridView.Application.Views
{
    public class ShowCodeModal : ReactiveWindow<ShowCodeModalViewModel>
    {
        public ShowCodeModal()
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