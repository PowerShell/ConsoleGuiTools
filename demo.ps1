# .Silent cls
# Out-ConsoleGridView (ocgv) from Microsoft.PowerShell.ConsoleGuiTools Demo - Press enter to move to next step in demo
# Example 1: Output processes to a grid view
Get-Process | Out-ConsoleGridView 
# .Silent cls
# Example 2: Display a formatted table in a grid view
Get-Process | Select-Object -Property Name, WorkingSet, PeakWorkingSet | Sort-Object -Property WorkingSet -Descending | Out-ConsoleGridView
# .Silent cls
# Example 3: Define the function 'killp' to kill a process
function killp { Get-Process | Out-ConsoleGridView -OutputMode Single -Filter $args[0] | Stop-Process -Id {$_.Id} }
killp 
# .Silent cls
# Example 3b: 'killp note' fitlers for "note" (e.g. notepad.exe)
killp note
# .Silent cls
# Example 4: Navigate PowerShell command history (Map this to F7 with https://github.com/gui-cs/F7History)
Get-History | Sort-Object -Descending -Property Id -Unique | Select-Object CommandLine -ExpandProperty CommandLine | Out-ConsoleGridView -OutputMode Single -Filter $line -Title "Command Line History"
# .Silent cls
#  Example 4: Use Show-ObjectTree to output processes to a tree view
Get-Process | Show-ObjectTree