// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Avalonia.Markup.Xaml;

namespace OutGridView.Application
{
    public class App : Avalonia.Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
