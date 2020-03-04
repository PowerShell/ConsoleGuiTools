Invoke-Build Build
pwsh -noprofile -command "Import-Module '$PSScriptRoot/module/Microsoft.PowerShell.GraphicalTools'; Get-Process | Out-GridView -PassThru"
