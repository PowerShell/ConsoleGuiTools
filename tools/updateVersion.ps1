# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param(
    [Parameter(Mandatory)]
    [semver]$Version,

    [Parameter(Mandatory)]
    [string]$Changes
)

git diff --staged --quiet --exit-code
if ($LASTEXITCODE -ne 0) {
    throw "There are staged changes in the repository. Please commit or reset them before running this script."
}

$v = "$($Version.Major).$($Version.Minor).$($Version.Patch)"

$Path = "ConsoleGuiTools.Common.props"
$f = Get-Content -Path $Path
$f = $f -replace '^(?<prefix>\s+<VersionPrefix>)(.+)(?<suffix></VersionPrefix>)$', "`${prefix}${v}`${suffix}"
$f = $f -replace '^(?<prefix>\s+<VersionSuffix>)(.*)(?<suffix></VersionSuffix>)$', "`${prefix}$($Version.PreReleaseLabel)`${suffix}"
$f | Set-Content -Path $Path
git add $Path

$Path = "src/Microsoft.PowerShell.ConsoleGuiTools/Microsoft.PowerShell.ConsoleGuiTools.psd1"
$f = Get-Content -Path $Path
$f = $f -replace "^(?<prefix>ModuleVersion\s+=\s+')(.+)(?<suffix>')`$", "`${prefix}${v}`${suffix}"
$f | Set-Content -Path $Path
git add $Path

git commit --edit --message "v$($Version): $Changes"
