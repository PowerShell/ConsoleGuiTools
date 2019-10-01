// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using OutGridView.Application.Models;
using ReactiveUI;
using DynamicData;
using DynamicData.ReactiveUI;
using System.Linq;
using System.Reactive;
using Avalonia;
using System.Reactive.Disposables;

namespace OutGridView.Application.ViewModels
{
    public class ShowCodeModalViewModel : ViewModelBase
    {
        public string FilterScript { get; set; }
        public ReactiveCommand<Unit, Unit> CopyToClipboardCommand { get; }

        private readonly string _placholderText = "Add a filter to see generated code";

        public ShowCodeModalViewModel(string filterScript)
        {
            if (String.IsNullOrEmpty(filterScript))
            {
                FilterScript = _placholderText;
            }
            else
            {
                FilterScript = filterScript;
            }

            CopyToClipboardCommand = ReactiveCommand.Create(CopyToClipboard);

            this.WhenActivated((CompositeDisposable disposables) =>
            {
            });
        }

        private void CopyToClipboard()
        {
            Avalonia.Application.Current.Clipboard.SetTextAsync(FilterScript);
        }
    }
}
