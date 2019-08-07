using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OutGridView.Application.ViewModels;
using System.Reactive.Disposables;
using ReactiveUI;


namespace OutGridView.Application.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
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