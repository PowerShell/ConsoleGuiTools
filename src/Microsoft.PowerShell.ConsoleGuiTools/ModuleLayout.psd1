@{
    RequiredBuildAssets = @{
        'Microsoft.PowerShell.ConsoleGuiTools' = @(
            'publish/Microsoft.PowerShell.ConsoleGuiTools.dll',
            'publish/Microsoft.PowerShell.ConsoleGuiTools.pdb',
            'publish/Microsoft.PowerShell.ConsoleGuiTools.psd1',
            'publish/Terminal.Gui.dll',
            'publish/NStack.dll'
        )

        'OutGridView.Models' = @(
            'publish/OutGridView.Models.dll',
            'publish/OutGridView.Models.pdb'
        )
    }

    NativeBuildAssets = @()
}
