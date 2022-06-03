#
# Copyright (c) Microsoft.
# Licensed under the MIT license.
#

@{

# Script module or binary module file associated with this manifest.
RootModule = 'Microsoft.PowerShell.ConsoleGuiTools.dll'

# Version number of this module.
ModuleVersion = '0.6.3'

# Supported PSEditions
CompatiblePSEditions = @( 'Core' )

# ID used to uniquely identify this module
GUID = '06028f35-8304-4460-ae73-306741982afe'

# Author of this module
Author = 'PowerShell Team'

# Company or vendor of this module
CompanyName = 'Microsoft'

# Copyright statement for this module
Copyright = '(c) Microsoft Corporation.'

# Description of the functionality provided by this module
Description = 'Cross-platform Console Gui Tools for PowerShell'

# Minimum version of the PowerShell engine required by this module
PowerShellVersion = '6.2'

# Name of the PowerShell host required by this module
# PowerShellHostName = ''

# Minimum version of the PowerShell host required by this module
# PowerShellHostVersion = ''

# Minimum version of Microsoft .NET Framework required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
# DotNetFrameworkVersion = ''

# Minimum version of the common language runtime (CLR) required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
# CLRVersion = ''

# Processor architecture (None, X86, Amd64) required by this module
# ProcessorArchitecture = ''

# Modules that must be imported into the global environment prior to importing this module
# RequiredModules = @()

# Assemblies that must be loaded prior to importing this module
# RequiredAssemblies = @()

# Script files (.ps1) that are run in the caller's environment prior to importing this module.
# ScriptsToProcess = @()

# Type files (.ps1xml) to be loaded when importing this module
# TypesToProcess = @()

# Format files (.ps1xml) to be loaded when importing this module
# FormatsToProcess = @()

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
NestedModules = @()

# Functions to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no functions to export.
FunctionsToExport = @()

# Cmdlets to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no cmdlets to export.
CmdletsToExport = @( 'Out-ConsoleGridView' )

# Variables to export from this module
VariablesToExport = '*'

# Aliases to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no aliases to export.
AliasesToExport = @( 'ocgv' )

# DSC resources to export from this module
# DscResourcesToExport = @()

# List of all modules packaged with this module
# ModuleList = @()

# List of all files packaged with this module
# FileList = @()

# Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
PrivateData = @{

    PSData = @{

        # Tags applied to this module. These help with module discovery in online galleries.
        Tags = @('Console', 'Gui', 'Out-GridView', 'Out-ConsoleGridView', 'Terminal.Gui', 'gui.cs', 'MacOS', 'Windows', 'Linux', 'PSEdition_Core')

        # A URL to the license for this module.
        LicenseUri = 'https://github.com/PowerShell/GraphicalTools/blob/master/LICENSE.txt'

        # A URL to the main website for this project.
        ProjectUri = 'https://github.com/PowerShell/GraphicalTools/'

        # A URL to an icon representing this module.
        # IconUri = ''

        # ReleaseNotes of this module
        ReleaseNotes = '# Release Notes

## v0.6.3

Unreleased!

## v0.6.2

Update Terminal.Gui to 1.0.

Disable mouse mode to fix bug with mouse movement being printed to console.

Gracefully fail when running in remote sessions.

## v0.6.1

Fix off-by-one error with ellipsis so columns should be better aligned.

## v0.6.0

Now supports `-Filter` parameter.

Updated to use the final release of `Terminal.Gui` v0.90 (which is feature complete for 1.0).

## v0.5.0

`Out-ConsoleGridView` has been totally refactored!

First off, no more silly F9 menu to accept!
All you have to do is hit `ENTER` to accept your selection or `ESC` to cancel.

Also, `-OutputMode` works as expected now. `Single` lets you only select one item. `Multiple` is the default.

## v0.4.1

* Fix filter indexing to return correct selected objects

## v0.4.0

* Regex filter
* Newlines rendering fix (Thanks @tig!)

## v0.3.0

* Windows OS support (PS 6.2+)
* dynamic column widths
* Fix arrow key issue on non-Windows

## v0.2.0

Initial Release
'

        # Prerelease string of this module
        # Prerelease = ''

        # Flag to indicate whether the module requires explicit user acceptance for install/update/save
        RequireLicenseAcceptance = $false

        # External dependent modules of this module
        # ExternalModuleDependencies = @()

    } # End of PSData hashtable

} # End of PrivateData hashtable

# HelpInfo URI of this module
# HelpInfoURI = ''

# Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
# DefaultCommandPrefix = ''

}
