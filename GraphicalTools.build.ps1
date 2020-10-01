
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [string[]]$ModuleName = @( 
        "Microsoft.PowerShell.GraphicalTools", 
        "Microsoft.PowerShell.ConsoleGuiTools" ) 
)

$script:IsUnix = $PSVersionTable.PSEdition -and $PSVersionTable.PSEdition -eq "Core" -and !$IsWindows

$script:TargetFramework = "netcoreapp3.0"
$script:RequiredSdkVersion = (Get-Content (Join-Path $PSScriptRoot 'global.json') | ConvertFrom-Json).sdk.version

$script:ModuleLayouts = @{}
foreach ($mn in $ModuleName) {
    $script:ModuleLayouts.$mn = Import-PowerShellDataFile -Path "$PSScriptRoot/src/$mn/ModuleLayout.psd1"
}

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

    foreach ($moduleLayout in $script:ModuleLayouts.Values) {
        foreach ($projName in $moduleLayout.RequiredBuildAssets.Keys) {
            exec { & $script:dotnetExe publish -c $Configuration "$PSScriptRoot/src/$projName/$projName.csproj" }
        }

        foreach ($nativeProj in $moduleLayout.NativeBuildAssets.Keys) {
            foreach ($targetPlatform in $moduleLayout.NativeBuildAssets[$nativeProj]) {
                $buildPropertyParams = if ($targetPlatform -eq "win-x64") {
                    "/property:IsWindows=true"
                }
                else {
                    "/property:IsWindows=false"
                }
                exec { & $script:dotnetExe publish -c $Configuration "$PSScriptRoot/src/$nativeProj/$nativeProj.csproj" -r $targetPlatform $buildPropertyParams }
            }
        }
    }
}

task Clean {
    #Remove Module Build
    Remove-Item $PSScriptRoot/module -Recurse -Force -ErrorAction Ignore

    foreach ($moduleLayout in $script:ModuleLayouts.Values) {
        foreach ($projName in $moduleLayout.RequiredBuildAssets.Keys) {
            exec { & $script:dotnetExe clean -c $Configuration "$PSScriptRoot/src/$projName/$projName.csproj" }
        }

        foreach ($projName in $moduleLayout.NativeBuildAssets.Keys) {
            exec { & $script:dotnetExe clean -c $Configuration "$PSScriptRoot/src/$projName/$projName.csproj" }
        }
    }

    foreach ($mn in $ModuleName) {
        Get-ChildItem "$PSScriptRoot\module\$mn\Commands\en-US\*-help.xml" -ErrorAction Ignore | Remove-Item -Force
    }
}

task LayoutModule -After Build {
    foreach ($mn in $ModuleName) {
        $moduleLayout = $script:ModuleLayouts[$mn]
        $moduleBinPath = "$PSScriptRoot/module/$mn/"

        # Create the destination dir
        $null = New-Item -Force $moduleBinPath -Type Directory

        # For each PSES subproject
        foreach ($projectName in $moduleLayout.RequiredBuildAssets.Keys) {
            # Get the project build dir path
            $basePath = [System.IO.Path]::Combine($PSScriptRoot, 'src', $projectName, 'bin', $Configuration, $script:TargetFramework)

            # For each asset in the subproject
            foreach ($bin in $moduleLayout.RequiredBuildAssets[$projectName]) {
                # Get the asset path
                $binPath = Join-Path $basePath $bin

                # Binplace the asset
                Copy-Item -Force -Verbose $binPath $moduleBinPath
            }
        }

        foreach ($projectName in $moduleLayout.NativeBuildAssets.Keys) {
            foreach ($targetPlatform in $moduleLayout.NativeBuildAssets[$projectName]) {
                $destDir = Join-Path $moduleBinPath $projectName $targetPlatform
    
                $null = New-Item -Force $destDir -Type Directory
    
                # Get the project build dir path
                $publishPath = [System.IO.Path]::Combine($PSScriptRoot, 'src', $projectName, 'bin', $Configuration, $script:TargetFramework, $targetPlatform, "publish\*" )
    
                Write-Host $publishPath
                # Binplace the asset
                Copy-Item -Recurse -Force  $publishPath $destDir
            }
        }

        Copy-Item -Force "$PSScriptRoot/README.md" $moduleBinPath
        Copy-Item -Force "$PSScriptRoot/LICENSE.txt" $moduleBinPath
    }
}

task BuildCmdletHelp {
    foreach ($mn in $ModuleName) {
        New-ExternalHelp -Path "$PSScriptRoot/docs/$mn" -OutputPath "$PSScriptRoot/module/$mn/en-US" -Force
    }
}

task PackageModule {
    foreach ($mn in $ModuleName) {
        Remove-Item "$PSScriptRoot/$mn.zip" -Force -ErrorAction Ignore
        Compress-Archive -Path "$PSScriptRoot/module/$mn/" -DestinationPath "$mn.zip" -CompressionLevel Optimal -Force
    }
}

task UploadArtifacts -If ($null -ne $env:TF_BUILD) {
    foreach ($mn in $ModuleName) {
        Copy-Item -Path "$PSScriptRoot/$mn.zip" -Destination "$env:BUILD_ARTIFACTSTAGINGDIRECTORY/$mn-$($env:AGENT_OS).zip"
    }
}

task . Clean, Build, BuildCmdletHelp, PackageModule, UploadArtifacts
