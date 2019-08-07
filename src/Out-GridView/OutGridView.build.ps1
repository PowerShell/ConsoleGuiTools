
$script:IsUnix = $PSVersionTable.PSEdition -and $PSVersionTable.PSEdition -eq "Core" -and !$IsWindows

$script:ModuleBinPath = "$PSScriptRoot/module/OutGridView/bin/"
$script:TargetFramework = "netcoreapp3.0"
$script:RequiredSdkVersion = "3.0.100-preview5-011568"
$script:Configuration = "Debug"

#TODO add other platforms
# $script:TargetPlatforms = @("win10-x64", "osx-x64", "linux-x64")
$script:TargetPlatforms = @("win10-x64")

$script:RequiredBuildAssets = @{
    $script:ModuleBinPath = @{
        'Cmdlet'      = @(
            'publish/OutGridViewCmdlet.dll',
            'publish/OutGridViewCmdlet.pdb'
        )

        'Models'      = @(
            'publish/OutGridViewModels.dll',
            'publish/OutGridViewModels.pdb'
        )
    }
}

$script:NativeBuildAssets = @(
    'Application' 
)

task SetupDotNet -Before Build {

    $dotnetPath = "$PSScriptRoot/.dotnet"
    $dotnetExePath = if ($script:IsUnix) { "$dotnetPath/dotnet" } else { "$dotnetPath/dotnet.exe" }
    $originalDotNetExePath = $dotnetExePath

    if (!(Test-Path $dotnetExePath)) {
        $installedDotnet = Get-Command dotnet -ErrorAction Ignore
        if ($installedDotnet) {
            $dotnetExePath = $installedDotnet.Source
        }
        else {
            $dotnetExePath = $null
        }
    }

    # Make sure the dotnet we found is the right version
    if ($dotnetExePath) {
        # dotnet --version can write to stderr, which causes builds to abort, therefore use --list-sdks instead.
        if ((& $dotnetExePath --list-sdks | ForEach-Object { $_.Split()[0] } ) -contains $script:RequiredSdkVersion) {
            $script:dotnetExe = $dotnetExePath
        }
        else {
            # Clear the path so that we invoke installation
            $script:dotnetExe = $null
        }
    }
    else {
        # Clear the path so that we invoke installation
        $script:dotnetExe = $null
    }

    if ($script:dotnetExe -eq $null) {

        Write-Host "`n### Installing .NET CLI $script:RequiredSdkVersion...`n" -ForegroundColor Green

        # The install script is platform-specific
        $installScriptExt = if ($script:IsUnix) { "sh" } else { "ps1" }

        # Download the official installation script and run it
        $installScriptPath = "$([System.IO.Path]::GetTempPath())dotnet-install.$installScriptExt"
        Invoke-WebRequest "https://raw.githubusercontent.com/dotnet/cli/v$script:RequiredSdkVersion/scripts/obtain/dotnet-install.$installScriptExt" -OutFile $installScriptPath
        $env:DOTNET_INSTALL_DIR = "$PSScriptRoot/.dotnet"

        if (!$script:IsUnix) {
            & $installScriptPath -Version $script:RequiredSdkVersion -InstallDir "$env:DOTNET_INSTALL_DIR"
        }
        else {
            & /bin/bash $installScriptPath -Version $script:RequiredSdkVersion -InstallDir "$env:DOTNET_INSTALL_DIR"
            $env:PATH = $dotnetExeDir + [System.IO.Path]::PathSeparator + $env:PATH
        }

        Write-Host "`n### Installation complete." -ForegroundColor Green
        $script:dotnetExe = $originalDotnetExePath
    }

    # This variable is used internally by 'dotnet' to know where it's installed
    $script:dotnetExe = Resolve-Path $script:dotnetExe
    if (!$env:DOTNET_INSTALL_DIR)
    {
        $dotnetExeDir = [System.IO.Path]::GetDirectoryName($script:dotnetExe)
        $env:PATH = $dotnetExeDir + [System.IO.Path]::PathSeparator + $env:PATH
        $env:DOTNET_INSTALL_DIR = $dotnetExeDir
    }

    Write-Host "`n### Using dotnet v$(& $script:dotnetExe --version) at path $script:dotnetExe`n" -ForegroundColor Green
}

function BuildForPlatform {
    param (
        [string]$TargetPlatform 
    )

    exec { & $script:dotnetExe publish -c $script:Configuration .\Cmdlet\OutGridViewCmdlet.csproj  }
    exec { & $script:dotnetExe publish -c $script:Configuration .\Models\OutGridViewModels.csproj }

    exec { & $script:dotnetExe publish -c $script:Configuration .\Application\OutGridViewApplication.csproj -r $TargetPlatform }
}

task Build {
    foreach($targetPlatform in $script:TargetPlatforms) {
        BuildForPlatform -TargetPlatform $targetPlatform
    }
}

task Clean -Before Build {
    #Remove Module Build
    Remove-Item $PSScriptRoot\module\OutGridView\bin -Recurse -Force -ErrorAction Ignore

    #Remove Project Build Folders
    Remove-Item $PSScriptRoot\Application\bin -Recurse -Force -ErrorAction Ignore
    Remove-Item $PSScriptRoot\Cmdlet\bin -Recurse -Force -ErrorAction Ignore
    Remove-Item $PSScriptRoot\Models\bin -Recurse -Force -ErrorAction Ignore
}

task LayoutModule -After Build {
    foreach ($destDir in $script:RequiredBuildAssets.Keys) {
        # Create the destination dir
        $null = New-Item -Force $destDir -Type Directory

        # For each PSES subproject
        foreach ($projectName in $script:RequiredBuildAssets[$destDir].Keys) {
            # Get the project build dir path
            $basePath = [System.IO.Path]::Combine($PSScriptRoot, $projectName, 'bin', $Configuration,  $script:TargetFramework)

            # For each asset in the subproject
            foreach ($bin in $script:RequiredBuildAssets[$destDir][$projectName]) {
                # Get the asset path
                $binPath = Join-Path $basePath $bin

                # Binplace the asset
                Copy-Item -Force -Verbose $binPath $destDir
            }
        }
    }

    foreach ($projectName in $script:NativeBuildAssets) {
        foreach($targetPlatform in $script:TargetPlatforms) {
            $destDir = Join-Path $script:ModuleBinPath $projectName $targetPlatform

            $null = New-Item -Force $destDir -Type Directory

            # Get the project build dir path
            $publishPath = [System.IO.Path]::Combine($PSScriptRoot, $projectName, 'bin', $Configuration,  $script:TargetFramework, $targetPlatform, "publish\*" )

            Write-Host $publishPath
            # Binplace the asset
            Copy-Item -Recurse -Force  $publishPath $destDir
        }
    }
}

task . Build

