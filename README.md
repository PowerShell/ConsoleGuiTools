# GraphicalTools

## Installation
```powershell
Install-Module Microsoft.PowerShell.GraphicalTools
```

## Features

### Cross-Platform
|Linux   |Windows   |Mac   |
|---|---|---|
| ![linux-gif](https://powershell.github.io/PowerShell-Blog/Images/2019-08-13-OutGridView-Returns/OutGridViewLinux.gif) |  ![window-gif](https://powershell.github.io/PowerShell-Blog/Images/2019-08-13-OutGridView-Returns/OutGridViewWindows.gif) | ![macos-gif](https://powershell.github.io/PowerShell-Blog/Images/2019-08-13-OutGridView-Returns/OutGridViewMac.gif)|

- Out-Gridview
    - View and filter objects
    - Generate resuable filter code


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

### Building the code from Visual Studio Code

Open the PowerShellGraphicalTools folder that you cloned locally and press <kbd>Ctrl+Shift+B</kbd>
(or <kbd>Cmd+Shift+B</kbd> on macOS).

## Contributions Welcome!

We would love to incorporate community contributions into this project.  If you would like to
contribute code, documentation, tests, or bug reports, please read our [Contribution Guide](http://powershell.github.io/GraphicalTools/CONTRIBUTING.html) to learn more.



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
