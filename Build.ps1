Invoke-Build
pwsh -c "Import-Module '$PSScriptRoot/module/GraphicalTools'; Get-Process | Out-GridView -PassThru"
