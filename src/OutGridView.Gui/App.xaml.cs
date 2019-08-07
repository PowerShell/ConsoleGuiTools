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