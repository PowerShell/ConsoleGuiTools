// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OutGridView.Application.ViewModels;
using ReactiveUI;

namespace OutGridView.Application.Views
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
