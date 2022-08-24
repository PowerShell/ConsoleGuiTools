# GraphicalTools - `Out-ConsoleGridView`

The GraphicalTools repo contains the `Out-ConsoleGridView` 
PowerShell Cmdlet providing console-based GUI experiences based on
[Terminal.Gui (gui.cs)](https://github.com/migueldeicaza/gui.cs).

_Note:_ A module named `Microsoft.PowerShell.GraphicalTools` used to be built and published out of this repo, but per [#101](https://github.com/PowerShell/GraphicalTools/issues/101) it is deprecated and unmaintained until such time that it can be rewritten on top of [.NET MAUI](https://devblogs.microsoft.com/dotnet/introducing-net-multi-platform-app-ui/).

## Installation

```powershell
Install-Module Microsoft.PowerShell.ConsoleGuiTools
```

## Features

Cross-platform! Use the cmdlet
[`Out-ConsoleGridview`](docs/Microsoft.PowerShell.ConsoleGuiTools/Out-ConsoleGridView.md)
to view and filter objects graphically.

![screenshot of Out-ConsoleGridView](docs/Microsoft.PowerShell.ConsoleGuiTools/ocgv.gif)

### Examples

Get a process ID:
```powershell
gps | ocgv -OutputMode Single
```

See the [F7 History](https://github.com/gui-cs/F7History) script to show the PowerShell command history when F7 is pressed.

## Development

### 1. Install PowerShell 7.2+

Install PowerShell 7.2+ with [these instructions](https://github.com/PowerShell/PowerShell#get-powershell).

### 2. Clone the GitHub repository

```powershell
git clone https://github.com/PowerShell/GraphicalTools.git
```

### 3. Install [Invoke-Build](https://github.com/nightroman/Invoke-Build)

```powershell
Install-Module InvokeBuild -Scope CurrentUser
```

Now you're ready to build the code.  You can do so in one of two ways:

### 4. Building the code from PowerShell

```powershell
PS ./GraphicalTools> Invoke-Build Build -ModuleName Microsoft.PowerShell.ConsoleGuiTools
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
PS ./GraphicalTools> code .
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
please read the [development section above](https://github.com/PowerShell/GraphicalTools#development)
to learn more.

## (Deprecated) Microsoft.PowerShell.GraphicalTools Architecture

`GraphicalTools` consists of 2 .NET Projects:

- ConsoleGuiTools - Cmdlet implementation for Out-ConsoleGridView
- OutGridView.Models - Contains data contracts between the GUI & Cmdlet

_Note:_ Previously, GraphicalTools also included the Avalonia-based `Out-GridView` which was implemented in `.\Microsoft.PowerShell.GraphicalTools` and `.\OutGridView.Gui`. These components have been deprecated (see note above).

## Maintainers

- [Andrew Schwartzmeyer](https://andschwa.com) - [@andschwa](https://github.com/andschwa)

Originally authored by [Tyler Leonhardt](http://twitter.com/tylerleonhardt).

## License

This project is [licensed under the MIT License](LICENSE).

## [Code of Conduct][conduct-md]

This project has adopted the [Microsoft Open Source Code of Conduct][conduct-code].
For more information see the [Code of Conduct FAQ][conduct-FAQ] or contact [opencode@microsoft.com][conduct-email] with any additional questions or comments.

[conduct-code]: https://opensource.microsoft.com/codeofconduct/
[conduct-FAQ]: https://opensource.microsoft.com/codeofconduct/faq/
[conduct-email]: mailto:opencode@microsoft.com
[conduct-md]: https://github.com/PowerShell/GraphicalTools/tree/master/CODE_OF_CONDUCT.md
