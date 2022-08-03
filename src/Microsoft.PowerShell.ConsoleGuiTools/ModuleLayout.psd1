@{
    RequiredBuildAssets = @{
        'Microsoft.PowerShell.ConsoleGuiTools' = @(
            'publish/Microsoft.PowerShell.ConsoleGuiTools.dll',
            'publish/Microsoft.PowerShell.ConsoleGuiTools.pdb',
            'publish/Microsoft.PowerShell.ConsoleGuiTools.psd1',
            'publish/Terminal.Gui.dll',
            'publish/NStack.dll'
        )

        'Microsoft.PowerShell.OutGridView.Models' = @(
            'publish/Microsoft.PowerShell.OutGridView.Models.dll',
            'publish/Microsoft.PowerShell.OutGridView.Models.pdb'
        )
    }

    NativeBuildAssets = @()
}
