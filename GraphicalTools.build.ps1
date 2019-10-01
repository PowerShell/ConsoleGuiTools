
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$script:IsUnix = $PSVersionTable.PSEdition -and $PSVersionTable.PSEdition -eq "Core" -and !$IsWindows

$script:ModuleName = "Microsoft.PowerShell.GraphicalTools"
$script:ModuleBinPath = "$PSScriptRoot/module/$script:ModuleName/"
$script:TargetFramework = "netcoreapp3.0"
$script:RequiredSdkVersion = "3.0.100-preview5-011568"
$script:TargetPlatforms = @("win-x64", "osx-x64", "linux-x64")

$script:RequiredBuildAssets = @{
    $script:ModuleBinPath = @{
        $script:ModuleName = @(
            "publish/$script:ModuleName.dll",
            "publish/$script:ModuleName.pdb",
            "publish/$script:ModuleName.psd1",
            "publish/$script:ModuleName.psm1"
        )

        'GraphicalTools.Models' = @(
            'publish/GraphicalTools.Models.dll',
            'publish/GraphicalTools.Models.pdb'
        )
    }
}

$script:NativeBuildAssets = @(
    'GraphicalTools.Gui' 
)

task SetupDotNet -Before Clean, Build {

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
        Invoke-WebRequest "https://dot.net/v1/dotnet-install.$installScriptExt" -OutFile $installScriptPath
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
    if (!$env:DOTNET_INSTALL_DIR) {
        $dotnetExeDir = [System.IO.Path]::GetDirectoryName($script:dotnetExe)
        $env:PATH = $dotnetExeDir + [System.IO.Path]::PathSeparator + $env:PATH
        $env:DOTNET_INSTALL_DIR = $dotnetExeDir
    }

    Write-Host "`n### Using dotnet v$(& $script:dotnetExe --version) at path $script:dotnetExe`n" -ForegroundColor Green
}

task Build {
    Remove-Item $PSScriptRoot/module -Recurse -Force -ErrorAction Ignore

    exec { & $script:dotnetExe publish -c $Configuration "$PSScriptRoot/src/$script:ModuleName/$script:ModuleName.csproj" }
    exec { & $script:dotnetExe publish -c $Configuration "$PSScriptRoot/src/GraphicalTools.Models/GraphicalTools.Models.csproj" }


    foreach ($targetPlatform in $script:TargetPlatforms) {
        $buildPropertyParams = if ($targetPlatform -eq "win-x64") {
            "/property:IsWindows=true"
        }
        else {
            "/property:IsWindows=false"
        }
        exec { & $script:dotnetExe publish -c $Configuration "$PSScriptRoot/src/GraphicalTools.Gui/GraphicalTools.Gui.csproj" -r $targetPlatform $buildPropertyParams }
    }
}

task Clean {
    #Remove Module Build
    Remove-Item $PSScriptRoot/module -Recurse -Force -ErrorAction Ignore

    exec { & $script:dotnetExe clean -c $Configuration "$PSScriptRoot/src/$script:ModuleName/$script:ModuleName.csproj" }
    exec { & $script:dotnetExe clean -c $Configuration "$PSScriptRoot/src/GraphicalTools.Models/GraphicalTools.Models.csproj" }
    exec { & $script:dotnetExe clean -c $Configuration "$PSScriptRoot/src/GraphicalTools.Gui/GraphicalTools.Gui.csproj" }

    Get-ChildItem "$PSScriptRoot\module\$script:ModuleName\Commands\en-US\*-help.xml" -ErrorAction Ignore | Remove-Item -Force -ErrorAction Ignore
}

task LayoutModule -After Build {
    foreach ($destDir in $script:RequiredBuildAssets.Keys) {
        # Create the destination dir
        $null = New-Item -Force $destDir -Type Directory

        # For each PSES subproject
        foreach ($projectName in $script:RequiredBuildAssets[$destDir].Keys) {
            # Get the project build dir path
            $basePath = [System.IO.Path]::Combine($PSScriptRoot, 'src', $projectName, 'bin', $Configuration, $script:TargetFramework)

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
        foreach ($targetPlatform in $script:TargetPlatforms) {
            $destDir = Join-Path $script:ModuleBinPath $projectName $targetPlatform

            $null = New-Item -Force $destDir -Type Directory

            # Get the project build dir path
            $publishPath = [System.IO.Path]::Combine($PSScriptRoot, 'src', $projectName, 'bin', $Configuration, $script:TargetFramework, $targetPlatform, "publish\*" )

            Write-Host $publishPath
            # Binplace the asset
            Copy-Item -Recurse -Force  $publishPath $destDir
        }
    }

    Copy-Item -Force "$PSScriptRoot/README.md" $script:ModuleBinPath
    Copy-Item -Force "$PSScriptRoot/LICENSE.txt" $script:ModuleBinPath
}

task BuildCmdletHelp {
    New-ExternalHelp -Path "$PSScriptRoot/docs" -OutputPath "$script:ModuleBinPath/en-US" -Force
}

task PackageModule {
    Remove-Item "$PSScriptRoot/$script:ModuleName.zip" -Force -ErrorAction Ignore
    Compress-Archive -Path $script:ModuleBinPath -DestinationPath "$script:ModuleName.zip" -CompressionLevel Optimal -Force
}

task UploadArtifacts -If ($null -ne $env:TF_BUILD) {
    Copy-Item -Path ".\$script:ModuleName.zip" -Destination "$env:BUILD_ARTIFACTSTAGINGDIRECTORY/$script:ModuleName-$($env:AGENT_OS).zip"
}

task . Clean, Build, BuildCmdletHelp, PackageModule, UploadArtifacts
