# Build script for buildling/testing from the commnad line. See tasks.json for how build is invoked within VS Code
# GraphicalTools includes two modules: Microsoft.PowerShell.GraphicalTools and Microsoft.PowerShell.ConsoleGuiTools
# To build them all leave -ModuleName off the `InvokeBuild` command (e.g. Invoke-Build Build).
# To build only one, specify it using the -ModuleName paramater (e.g. Invoke-Build Build -ModuleName Microsoft.PowerShell.ConsoleGuiTools).

# Build...
Invoke-Build Build -ModuleName Microsoft.PowerShell.ConsoleGuiTools

# Run what was built...
# pwsh -noprofile -command "Import-Module -verbose '$PSScriptRoot/module/Microsoft.PowerShell.GraphicalTools'; Get-Process | Out-GridView -PassThru"
pwsh -noprofile -command "Import-Module -verbose '$PSScriptRoot/module/Microsoft.PowerShell.ConsoleGuiTools'; Get-PSProfile | Out-ConsoleGridView -OutputMode Single -Title 'PS Profiles' -Filter power"