Invoke-Build Build
pwsh -c "Import-Module '$PSScriptRoot/module/Microsoft.PowerShell.GraphicalTools'; Get-Process | Out-GridView -PassThru"
