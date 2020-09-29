Invoke-Build Build
pwsh -noprofile -command "Import-Module -verbose '$PSScriptRoot/module/Microsoft.PowerShell.GraphicalTools'; Get-Process | Out-GridView -PassThru"
pwsh -noprofile -command "Import-Module -verbose '$PSScriptRoot/module/Microsoft.PowerShell.ConsoleGuiTools'; Get-PSProfile | Out-ConsoleGridView -OutputMode Single -Title 'PS Profiles' -Filter power"