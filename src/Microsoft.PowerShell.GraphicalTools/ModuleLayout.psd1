@{
    RequiredBuildAssets = @{
        'Microsoft.PowerShell.GraphicalTools' = @(
            "publish/Microsoft.PowerShell.GraphicalTools.dll",
            "publish/Microsoft.PowerShell.GraphicalTools.pdb",
            "publish/Microsoft.PowerShell.GraphicalTools.psd1",
            "publish/Microsoft.PowerShell.GraphicalTools.psm1"
        )

        'Microsoft.PowerShell.OutGridView.Models' = @(
            'publish/Microsoft.PowerShell.OutGridView.Models.dll',
            'publish/Microsoft.PowerShell.OutGridView.Models.pdb'
        )
    }

    NativeBuildAssets = @{
        'OutGridView.Gui' = @("win-x64", "osx-x64", "linux-x64")
    }
}
