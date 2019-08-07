Push-Location (Join-Path $PSScriptRoot ..)

if(!(Get-Module -ListAvailable InvokeBuild)) {
    Install-Module InvokeBuild -Force -Scope CurrentUser
}

Invoke-Build
Pop-Location
