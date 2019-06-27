using Avalonia;
using Avalonia.Markup.Xaml;
using OutGridView.ViewModels;
using System.Reactive.Disposables;
using ReactiveUI;
using DynamicData;


namespace OutGridView.Views
{
    public class ColumnSelectDialog : ReactiveWindow<ColumnSelectDialogViewModel>
    {
        public SourceList<string> ColumnOptions { get; set; }
        public ColumnSelectDialog(SourceList<string> columnOptions)
        {
            ColumnOptions = columnOptions;
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