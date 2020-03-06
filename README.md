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

Not currently available on the PowerShell Gallery.

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

#### Cross-Platform

- Out-ConsoleGridview
    - View and filter objects

## Development

### 1. Install PowerShell 6.2+

Install PowerShell 6.2+ with [these instructions](https://github.com/PowerShell/PowerShell#get-powershell).

### 3. Clone the GitHub repository:

```
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

## Contributions Welcome!

We would love to incorporate community contributions into this project.  If you would like to
contribute code, documentation, tests, or bug reports, please read our [Contribution Guide](http://powershell.github.io/GraphicalTools/CONTRIBUTING.html) to learn more.

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
