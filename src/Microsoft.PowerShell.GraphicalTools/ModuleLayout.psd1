@{
    RequiredBuildAssets = @{
        'Microsoft.PowerShell.GraphicalTools' = @(
            "publish/Microsoft.PowerShell.GraphicalTools.dll",
            "publish/Microsoft.PowerShell.GraphicalTools.pdb",
            "publish/Microsoft.PowerShell.GraphicalTools.psd1",
            "publish/Microsoft.PowerShell.GraphicalTools.psm1"
        )

        'OutGridView.Models' = @(
            'publish/OutGridView.Models.dll',
            'publish/OutGridView.Models.pdb'
        )
    }

    NativeBuildAssets = @{
        'OutGridView.Gui' = @("win-x64", "osx-x64", "linux-x64")
    }
}
