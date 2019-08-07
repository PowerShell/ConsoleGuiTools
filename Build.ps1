Invoke-Build Build
pwsh -c "Import-Module '$PSScriptRoot/module/GraphicalTools'; Get-Process | Out-CrossGridView -PassThru"
