
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

task FindDotNet -Before Clean, Build {
    Assert (Get-Command dotnet -ErrorAction SilentlyContinue) "The dotnet CLI was not found, please install it: https://aka.ms/dotnet-cli"
    $DotnetVersion = dotnet --version
    Assert ($?) "The required .NET SDK was not found, please install it: https://aka.ms/dotnet-cli"
    Write-Host "Using dotnet $DotnetVersion at path $((Get-Command dotnet).Source)" -ForegroundColor Green
}

task Clean {
    Remove-BuildItem ./module, ./out
    Push-Location src/Microsoft.PowerShell.ConsoleGuiTools
    Invoke-BuildExec { & dotnet clean }
    Pop-Location
}

task Build {
    New-Item -ItemType Directory -Force ./module | Out-Null

    Push-Location src/Microsoft.PowerShell.ConsoleGuiTools
    Invoke-BuildExec { & dotnet publish --configuration $Configuration --output publish }
    $Assets = $(
        "../../README.md",
        "../../LICENSE.txt",
        "./publish/Microsoft.PowerShell.ConsoleGuiTools.dll",
        "./publish/Microsoft.PowerShell.ConsoleGuiTools.psd1",
        "./publish/Microsoft.PowerShell.OutGridView.Models.dll",
        "./publish/Terminal.Gui.dll",
        "./publish/NStack.dll")
    $Assets | ForEach-Object {
        Copy-Item -Force -Path $_ -Destination ../../module
    }
    Pop-Location

    New-ExternalHelp -Path docs/Microsoft.PowerShell.ConsoleGuiTools -OutputPath module/en-US -Force
}

task Package {
    New-Item -ItemType Directory -Force ./out | Out-Null
    if (-Not (Get-PSResourceRepository -Name ConsoleGuiTools -ErrorAction SilentlyContinue)) {
        Register-PSResourceRepository -Name ConsoleGuiTools -Uri ./out
    }
    Publish-PSResource -Path ./module -Repository ConsoleGuiTools -Verbose
}

task . Clean, Build
