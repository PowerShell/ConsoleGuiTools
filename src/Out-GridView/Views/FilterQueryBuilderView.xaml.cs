using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OutGridView.ViewModels;
using ReactiveUI;

namespace OutGridView.Views
{
    public class FilterQueryBuilderView : ReactiveUserControl<FilterQueryBuilderViewModel>
    {
        public Button ClearFiltersButton => this.FindControl<Button>("ClearFiltersButton");
        public ComboBox AddFilterComboBox => this.FindControl<ComboBox>("AddFilterComboxBox");

        public FilterQueryBuilderView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables =>
            {
                this.BindCommand(ViewModel, x => x.ClearFiltersCommand, x => x.ClearFiltersButton);

                this.BindCommand(ViewModel, x => x.AddFilterCommand, x => x.AddFilterComboBox, x => x.SelectedAddColumn, nameof(AddFilterComboBox.SelectionChanged));
            });

            AvaloniaXamlLoader.Load(this);

        }
    }
}