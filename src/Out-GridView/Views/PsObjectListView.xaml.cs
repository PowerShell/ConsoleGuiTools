using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OutGridView.ViewModels;
using System.Reactive.Disposables;
using ReactiveUI;
using System.Management.Automation;

namespace OutGridView.Views
{
    public partial class PsObjectListView : ReactiveUserControl<PsObjectListViewModel>
    {
        public DataGrid DataGrid => this.FindControl<DataGrid>("DataGrid");

        public PsObjectListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated((CompositeDisposable disposables) =>
              {
                  this.WhenAnyValue(x => x.DataGrid.SelectedItem)
                    .BindTo(this, x => x.ViewModel.SelectedObject);

              });
            AvaloniaXamlLoader.Load(this);


        }
    }
}