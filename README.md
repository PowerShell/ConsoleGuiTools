# GraphicalTools

The GraphicalTools repo contains several different graphical-related PowerShell modules including:

* `Microsoft.PowerShell.GraphicalTools` - A module that provides GUI experiences based on Avalonia.
* `Microsoft.PowerShell.ConsoleGuiTools` - A module that provides console-based GUI experiences based on gui.cs.

## Installation

### Microsoft.PowerShell.GraphicalTools

```powershell
Install-Module Microsoft.PowerShell.GraphicalTools
```

### Microsoft.PowerShell.ConsoleGuiTools

```powershell
Install-Module Microsoft.PowerShell.ConsoleGuiTools
```

## Features

### Microsoft.PowerShell.GraphicalTools

#### Cross-Platform
|Linux   |Windows   |Mac   |
|---|---|---|
| ![linux-gif](https://powershell.github.io/PowerShell-Blog/Images/2019-08-13-OutGridView-Returns/OutGridViewLinux.gif) |  ![window-gif](https://powershell.github.io/PowerShell-Blog/Images/2019-08-13-OutGridView-Returns/OutGridViewWindows.gif) | ![macos-gif](https://powershell.github.io/PowerShell-Blog/Images/2019-08-13-OutGridView-Returns/OutGridViewMac.gif)|

- Out-Gridview
    - View and filter objects
    - Generate reusable filter code

### Microsoft.PowerShell.ConsoleGuiTools

![screenshot of Out-ConsoleGridView](https://pbs.twimg.com/media/ESyWfiqUYAA_t6q?format=jpg&name=medium)

#### Cross-Platform

- Out-ConsoleGridview
    - View and filter objects

## Development

### 1. Install PowerShell 6.2+

Install PowerShell 6.2+ with [these instructions](https://github.com/PowerShell/PowerShell#get-powershell).

### 3. Clone the GitHub repository:

```powershell
git clone https://github.com/PowerShell/GraphicalTools.git
```

### 4. Install [Invoke-Build](https://github.com/nightroman/Invoke-Build)

```powershell
Install-Module InvokeBuild -Scope CurrentUser
```

Now you're ready to build the code.  You can do so in one of two ways:

### Building the code from PowerShell

```powershell
PS C:\path\to\GraphicalTools> Invoke-Build Build
```

> Note: You can build a single module using the -ModuleName parameter:
>
> ```powershell
> Invoke-Build Build -ModuleName Microsoft.PowerShell.ConsoleGuiTools
> ```

From there you can import the module that you just built for example:

```powershell
Import-Module .\module\Microsoft.PowerShell.ConsoleGuiTools
```

And then run the cmdlet you want to test, for example:

```powershell
Get-Process | Out-ConsoleGridView
```

> NOTE: If you change the code and rebuild the project, you'll need to launch a
> _new_ PowerShell process since the dll is already loaded and can't be unloaded.

### Debugging in Visual Studio Code


```powershell
PS C:\path\to\GraphicalTools> code .
```

Build by hitting `Ctrl-Shift-b` in VS Code.

To debug:

In a Powershell session in the `c:\path\to\GraphicalTools` directory, run `pwsh` (thus nesting powershell).

Then do the folowing:

```powershell
Import-Module .\module\Microsoft.PowerShell.ConsoleGuiTools
$pid
```

This will import the latest built DLL and output the process ID you'll need for debugging. Copy this ID to the clipboard.

In VScode, set your breakpoints, etc... Then hit `F5`. In the VScode search box, paste the value printed by `$pid`. You'll see something like `pwsh.exe 18328`. Click that and the debug session will start.

In the Powershell session run your commands; breakpoints will be hit, etc...

When done, run `exit` to exit the nested PowerShell and run `pwsh` again. This unloads the DLL. Repeat.


## Contributions Welcome!

We would love to incorporate community contributions into this project.  If you would like to
contribute code, documentation, tests, or bug reports, please read the [development section above](https://github.com/PowerShell/GraphicalTools#development) to learn more.

## Microsoft.PowerShell.GraphicalTools Architecture

Due to the quirks of the PowerShell threading implementation, the design of GUIs in this application are non-standard. The cmdlet invokes an Avalonia application as a separate process to guarantee the GUI is running on the main thread. Graphical tools therefore consists of 3 .NET Projects. 

- Microsoft.PowerShell.GraphicalTools - Cmdlet implementations
- OutGridView.Gui - Implementation of the Out-GridView window
- OutGridView.Models - Contains data contracts between the GUI & Cmdlet

## Maintainers

- [Tyler Leonhardt](https://github.com/tylerleonhardt) - [@TylerLeonhardt](http://twitter.com/tylerleonhardt)

## License

This project is [licensed under the MIT License](LICENSE).

## [Code of Conduct][conduct-md]

This project has adopted the [Microsoft Open Source Code of Conduct][conduct-code].
For more information see the [Code of Conduct FAQ][conduct-FAQ] or contact [opencode@microsoft.com][conduct-email] with any additional questions or comments.

[conduct-code]: https://opensource.microsoft.com/codeofconduct/
[conduct-FAQ]: https://opensource.microsoft.com/codeofconduct/faq/
[conduct-email]: mailto:opencode@microsoft.com
[conduct-md]: https://github.com/PowerShell/GraphicalTools/tree/master/CODE_OF_CONDUCT.md
