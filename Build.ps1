# Build script for buildling/testing from the commnad line. See tasks.json for how build is invoked within VS Code
# GraphicalTools includes two modules: Microsoft.PowerShell.GraphicalTools and Microsoft.PowerShell.ConsoleGuiTools
# To build them all leave -ModuleName off the `InvokeBuild` command (e.g. Invoke-Build Build).
# To build only one, specify it using the -ModuleName paramater (e.g. Invoke-Build Build -ModuleName Microsoft.PowerShell.ConsoleGuiTools).
$ModuleName = "Microsoft.PowerShell.ConsoleGuiTools"
$BuildPath = "$PSScriptRoot/module/$ModuleName"
$PsdPath = "src/$ModuleName/$ModulePath/$($ModuleName).psd1"

# Assume this is the first build
$build = 0

$psd1Content = Get-Content $PsdPath -Raw -ErrorAction SilentlyContinue
if ($psd1Content) {
    # Extract the ModuleVersion from the .psd1 content using regular expression
    if ($psd1Content -match "ModuleVersion\s+=\s+'(.*?)'") {
        $prevVersion = $Matches[1]
        $prevVersionParts = $prevVersion -split '\.'
        $build = [int]$prevVersionParts[3] + 1
        $ModuleVersion = "{0}.{1}.{2}.{3}" -f $prevVersionParts[0], $prevVersionParts[1], $prevVersionParts[2], $build
    }
    else {
        "No previous version found. Assuming this is the first build."
        # Get the ModuleVersion using dotnet-gitversion
        $prevVersion = "1.0.0.0"
        $ModuleVersion = "$($prevVersion)"
    }
    "Rewriting $PsdPath with new ModuleVersion: $ModuleVersion"
    $updatedpsd1Content = $psd1Content -replace "ModuleVersion\s+=\s+'([\d\.]+)'", "ModuleVersion = '$ModuleVersion'"
    $updatedpsd1Content | Out-File -FilePath $PsdPath -Encoding ascii
}
else {
    throw "$PsdPath not found."
}

"Buildihg $ModuleName..."
Invoke-Build Build -ModuleName $ModuleName

# Publish to a local PSRepository to enable downstream dependenies to use development builds
# - If `local` doesn't exist, create with `Register-PSRepository -Name local -SourceLocation "~/psrepo" -InstallationPolicy Trusted`
$localRepository = Get-PSRepository | Where-Object { $_.Name -eq 'local' }
if ($localRepository) {    
    $localRepositoryPath = $localRepository | Select-Object -ExpandProperty SourceLocation
    # Un-publishing $ModuleName from local repository at $localRepositoryPath"
    Remove-Item "${localRepositoryPath}/${ModuleName}.{$ModuleVersion}.nupkg" -Recurse -Force -ErrorAction SilentlyContinue
    "Publishing ${localRepositoryPath}/${ModuleName}.$ModuleVersion.nupkg to `local'"
    Publish-Module -Path $BuildPath  -Repository 'local'
}


# Run what was built as a bit of test of:
# - Scale: recursive ls of the project
# - Filter: proving regex works
# - SelectMultiple
$testCommand = "Get-ChildItem -Recurse | Out-ConsoleGridView -Debug -OutputMode Multiple -Title 'Imported Modules' -Filter \.xml"
"Running test in new pwsh session: $testCommand"
pwsh -noprofile -command "Import-Module -verbose $BuildPath; $testCommand"
"Test exited. Build complete."
