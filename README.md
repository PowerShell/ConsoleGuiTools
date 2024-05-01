# ConsoleGuiTools - `Out-ConsoleGridView` and `Show-ObjectTree`

This repo contains the `Out-ConsoleGridView` 
PowerShell Cmdlet providing console-based GUI experiences based on
[Terminal.Gui (gui.cs)](https://github.com/gui-cs/Terminal.Gui).

_Note:_ A module named `Microsoft.PowerShell.GraphicalTools` used to be built and published out of this repo, but per [#101](https://github.com/PowerShell/ConsoleGuiTools/issues/101) it is deprecated and unmaintained until such time that it can be rewritten on top of [.NET MAUI](https://devblogs.microsoft.com/dotnet/introducing-net-multi-platform-app-ui/).

## Installation

```powershell
Install-Module Microsoft.PowerShell.ConsoleGuiTools
```

## Features

* [`Out-ConsoleGridview`](docs/Microsoft.PowerShell.ConsoleGuiTools/Out-ConsoleGridView.md) - Send objects to a grid view window for interactive filtering and sorting.
* [`Show-ObjectTree`](docs/Microsoft.PowerShell.ConsoleGuiTools/Show-ObjectTree.md) - Send objects to a tree view window for interactive filtering and sorting.

* Cross-platform - Works on any platform that supports PowerShell 7.2+.
* Interactive - Use the mouse and keyboard to interact with the grid or tree view.
* Filtering - Filter the data using the built-in filter box.
* Sorting - Sort the data by clicking on the column headers.
* Multiple Selection - Select multiple items and send them down the pipeline.
* Customizable - Customize the grid view window with the built-in parameters.

![Demo GIF](docs/Microsoft.PowerShell.ConsoleGuiTools/ocgv.gif)

## Examples

### Example 1: Output processes to a grid view

```PowerShell
Get-Process | Out-ConsoleGridView
```

This command gets the processes running on the local computer and sends them to a grid view window.

### Example 2: Use a variable to output processes to a grid view

```PowerShell
$P = Get-Process
$P | Out-ConsoleGridView -OutputMode Single
```

This command also gets the processes running on the local computer and sends them to a grid view window.

The first command uses the Get-Process cmdlet to get the processes on the computer and then saves the process objects in the $P variable.

The second command uses a pipeline operator to send the $P variable to **Out-ConsoleGridView**.

By specifying `-OutputMode Single` the grid view window will be restricted to a single selection, ensuring no more than a single object is returned.

### Example 3: Display a formatted table in a grid view

```PowerShell
Get-Process | Select-Object -Property Name, WorkingSet, PeakWorkingSet | Sort-Object -Property WorkingSet -Descending | Out-ConsoleGridView
```

This command displays a formatted table in a grid view window.

It uses the Get-Process cmdlet to get the processes on the computer.

Then, it uses a pipeline operator (|) to send the process objects to the Select-Object cmdlet.
The command uses the **Property** parameter of **Select-Object** to select the Name, WorkingSet, and PeakWorkingSet properties to be displayed in the table.

Another pipeline operator sends the filtered objects to the Sort-Object cmdlet, which sorts them in descending order by the value of the **WorkingSet** property.

The final part of the command uses a pipeline operator (|) to send the formatted table to **Out-ConsoleGridView**.

You can now use the features of the grid view to search, sort, and filter the data.

### Example 4: Save output to a variable, and then output a grid view

```PowerShell
($A = Get-ChildItem -Path $pshome -Recurse) | Out-ConsoleGridView
```

This command saves its output in a variable and sends it to **Out-ConsoleGridView**.

The command uses the Get-ChildItem cmdlet to get the files in the Windows PowerShell installation directory and its subdirectories.
The path to the installation directory is saved in the $pshome automatic variable.

The command uses the assignment operator (=) to save the output in the $A variable and the pipeline operator (|) to send the output to **Out-ConsoleGridView**.

The parentheses in the command establish the order of operations.
As a result, the output from the Get-ChildItem command is saved in the $A variable before it is sent to **Out-ConsoleGridView**.

### Example 5: Output processes for a specified computer to a grid view

```PowerShell
Get-Process -ComputerName "Server01" | ocgv -Title "Processes - Server01"
```

This command displays the processes that are running on the Server01 computer in a grid view window.

The command uses `ocgv`, which is the built-in alias for the **Out-ConsoleGridView** cmdlet, it uses the *Title* parameter to specify the window title.

### Example 6: Define a function to kill processes using a graphical chooser

```PowerShell
function killp { Get-Process | Out-ConsoleGridView -OutputMode Single -Filter $args[0] | Stop-Process -Id {$_.Id} }
killp note
```
This example shows defining a function named `killp` that shows a grid view of all running processes and allows the user to select one to kill it.

The example uses the `-Filter` paramter to filter for all proceses with a name that includes `note` (thus highlighting `Notepad` if it were running. Selecting an item in the grid view and pressing `ENTER` will kill that process. 

### Example 7: Pass multiple items through Out-ConsoleGridView

```PowerShell
Get-Process | Out-ConsoleGridView -PassThru | Export-Csv -Path .\ProcessLog.csv
```

This command lets you select multiple processes from the **Out-ConsoleGridView** window.
The processes that you select are passed to the **Export-Csv** command and written to the ProcessLog.csv file.

The command uses the *PassThru* parameter of **Out-ConsoleGridView**, which lets you send multiple items down the pipeline.
The *PassThru* parameter is equivalent to using the Multiple value of the *OutputMode* parameter.

### Example 8: Use F7 as "Show Command History"

Add [gui-cs/F7History](https://github.com/gui-cs/F7History) to your Powershell profile.

Press `F7` to see the history for the current PowerShell instance

Press `Shift-F7` to see the history for all PowerShell instances.

Whatever you select within `Out-ConsoleGridView` will be inserted on your command line. 

Whatever was typed on the command line prior to hitting `F7` or `Shift-F7` will be used as a filter.

### Example 9: Output processes to a tree view

```PowerShell
PS C:\> Get-Process | Show-ObjectTree
```

This command gets the processes running on the local computer and sends them to a tree view window.

Use right arrow when a row has a `+` symbol to expand the tree. Left arrow will collapse the tree.

## Development

### 1. Install PowerShell 7.2+

Install PowerShell 7.2+ with [these instructions](https://github.com/PowerShell/PowerShell#get-powershell).

### 2. Clone the GitHub repository

```powershell
git clone https://github.com/PowerShell/ConsoleGuiTools.git
```

### 3. Install [Invoke-Build](https://github.com/nightroman/Invoke-Build)

```powershell
Install-Module InvokeBuild -Scope CurrentUser
```

Now you're ready to build the code.  You can do so in one of two ways:

### 4. Building the code from PowerShell

```powershell
pushd ./ConsoleGuiTools
Invoke-Build Build -ModuleName Microsoft.PowerShell.ConsoleGuiTools
popd
```

From there you can import the module that you just built for example (start a fresh `pwsh` instance first so you can unload the module with an `exit`; otherwise building again may fail because the `.dll` will be held open):

```powershell
pwsh
Import-Module ./module/Microsoft.PowerShell.ConsoleGuiTools
```

And then run the cmdlet you want to test, for example:

```powershell
Get-Process | Out-ConsoleGridView
exit
```

> NOTE: If you change the code and rebuild the project, you'll need to launch a
> _new_ PowerShell process since the DLL is already loaded and can't be unloaded.

### 5. Debugging in Visual Studio Code

```powershell
code ./ConsoleGuiTools
```

Build by hitting `Ctrl-Shift-B` in VS Code.

Set a breakpoint and hit `F5` to start the debugger.

Click on the VS Code "TERMINAL" tab and type your command that starts `Out-ConsoleGridView`, e.g.

```powershell
ls | ocgv
```

Your breakpoint should be hit.

## Contributions Welcome

We would love to incorporate community contributions into this project.  If
you would like to contribute code, documentation, tests, or bug reports,
please read the [development section above](https://github.com/PowerShell/ConsoleGuiTools#development)
to learn more.

## Microsoft.PowerShell.ConsoleGuiTools Architecture

`ConsoleGuiTools` consists of 2 .NET Projects:

- ConsoleGuiTools - Cmdlet implementation for Out-ConsoleGridView
- OutGridView.Models - Contains data contracts between the GUI & Cmdlet

_Note:_ Previously, this repo included `Microsoft.PowerShell.GraphicalTools` which included the Avalonia-based `Out-GridView` (implemented in `.\Microsoft.PowerShell.GraphicalTools` and `.\OutGridView.Gui`). These components have been deprecated (see note above).

## Maintainers

- [Andy Jordan](https://andyleejordan.com) - [@andyleejordan](https://github.com/andyleejordan)

Originally authored by [Tyler Leonhardt](http://twitter.com/tylerleonhardt).

## License

This project is [licensed under the MIT License](LICENSE).

## [Code of Conduct][conduct-md]

This project has adopted the [Microsoft Open Source Code of Conduct][conduct-code].
For more information see the [Code of Conduct FAQ][conduct-FAQ] or contact [opencode@microsoft.com][conduct-email] with any additional questions or comments.

[conduct-code]: https://opensource.microsoft.com/codeofconduct/
[conduct-FAQ]: https://opensource.microsoft.com/codeofconduct/faq/
[conduct-email]: mailto:opencode@microsoft.com
[conduct-md]: https://github.com/PowerShell/ConsoleGuiTools/tree/master/CODE_OF_CONDUCT.md
