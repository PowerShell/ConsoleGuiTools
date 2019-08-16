Push-Location (Join-Path $PSScriptRoot ..)

if (!(Get-Module -ListAvailable InvokeBuild)) {
    Install-Module InvokeBuild -Force -Scope CurrentUser
}

if (!(Get-Module -ListAvailable PlatyPS)) {
    Install-Module PlatyPS -Force -Scope CurrentUser
}

Invoke-Build -Configuration Release
Pop-Location
