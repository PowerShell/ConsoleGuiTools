using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OutGridView.ViewModels;
using System.Reactive.Disposables;
using ReactiveUI;


namespace OutGridView.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            System.Console.WriteLine("Main window rendered");

            this.WhenActivated((CompositeDisposable disposables) =>
                {
                    System.Console.WriteLine("Main window activated");
                });
            AvaloniaXamlLoader.Load(this);
        }
    }
}