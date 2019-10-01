// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using OutGridView.Application.ViewModels;
using OutGridView.Application.Views;
using Avalonia;

namespace OutGridView.Application.Services
{
    public class ModalService
    {
        public void ShowCodeModal(string filterString)
        {
            var showCodeModal = new ShowCodeModal
            {
                DataContext = new ShowCodeModalViewModel(filterString)
            };

            showCodeModal.ShowDialog(Avalonia.Application.Current.MainWindow);
        }
    }
}
