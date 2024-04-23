
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [string[]]$ModuleName = @("Microsoft.PowerShell.ConsoleGuiTools" )
)

$script:TargetFramework = "net6.0"

$script:ModuleLayouts = @{}
foreach ($mn in $ModuleName) {
    $script:ModuleLayouts.$mn = Import-PowerShellDataFile -Path "$PSScriptRoot/src/$mn/ModuleLayout.psd1"
}

task FindDotNet -Before Clean, Build {
    Assert (Get-Command dotnet -ErrorAction SilentlyContinue) "dotnet not found, please install it: https://aka.ms/dotnet-cli"

    # Strip out semantic version metadata so it can be cast to `Version`
    [Version]$existingVersion, $null = (dotnet --version) -split " " -split "-"
    Assert ($existingVersion -ge [Version]("6.0")) ".NET SDK 6.0 or higher is required, please update it: https://aka.ms/dotnet-cli"

    Write-Host "Using dotnet v$(dotnet --version) at path $((Get-Command dotnet).Source)" -ForegroundColor Green
}

task Build {
    Remove-Item $PSScriptRoot/module -Recurse -Force -ErrorAction Ignore

    foreach ($moduleLayout in $script:ModuleLayouts.Values) {
        foreach ($projName in $moduleLayout.RequiredBuildAssets.Keys) {
            exec { & dotnet publish -c $Configuration "$PSScriptRoot/src/$projName/$projName.csproj" }
        }

        foreach ($nativeProj in $moduleLayout.NativeBuildAssets.Keys) {
            foreach ($targetPlatform in $moduleLayout.NativeBuildAssets[$nativeProj]) {
                $buildPropertyParams = if ($targetPlatform -eq "win-x64") {
                    "/property:IsWindows=true"
                }
                else {
                    "/property:IsWindows=false"
                }
                exec { & dotnet publish -c $Configuration "$PSScriptRoot/src/$nativeProj/$nativeProj.csproj" -r $targetPlatform $buildPropertyParams }
            }
        }
    }
}

task Clean {
    Remove-BuildItem $PSScriptRoot/module

    foreach ($moduleLayout in $script:ModuleLayouts.Values) {
        foreach ($projName in $moduleLayout.RequiredBuildAssets.Keys) {
            exec { & dotnet clean -c $Configuration "$PSScriptRoot/src/$projName/$projName.csproj" }
        }

        foreach ($projName in $moduleLayout.NativeBuildAssets.Keys) {
            exec { & dotnet clean -c $Configuration "$PSScriptRoot/src/$projName/$projName.csproj" }
        }
    }
}

task LayoutModule -After Build {
    foreach ($mn in $ModuleName) {
        $moduleLayout = $script:ModuleLayouts[$mn]
        $moduleBinPath = "$PSScriptRoot/module/$mn/"

        # Create the destination dir
        $null = New-Item -Force $moduleBinPath -Type Directory

        # For each subproject
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

task . Clean, Build, BuildCmdletHelp
