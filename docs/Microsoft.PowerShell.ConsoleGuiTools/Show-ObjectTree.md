---
external help file: ConsoleGuiToolsModule.dll-Help.xml
keywords: powershell,cmdlet
locale: en-us
Module Name: Microsoft.PowerShell.ConsoleGuiTools
ms.date: 07/20/2023
schema: 2.0.0
title: Show-ObjectTree
---

# Show-ObjectTree

## SYNOPSIS

Sends output to an interactive tree in the same console window.

## SYNTAX

```PowerShell
 Show-ObjectTree [-InputObject <psobject>] [-Title <string>] [-OutputMode {None | Single |
    Multiple}] [-Filter <string>] [-MinUi] [<CommonParameters>]
```

## DESCRIPTION

The **Show-ObjectTree** cmdlet sends the output from a command to a tree view window where the output is displayed in an interactive tree.

You can use the following features of the tree to examine your data:

- Quick Filter. Use the Filter box at the top of the window to search the text in the tree. You can search for literals or multiple words. You can use the `-Filter` command to pre-populate the Filter box. The filter uses regular expressions.

For instructions for using these features, type `Get-Help Show-ObjectTree -Full` and see How to Use the Tree View Window Features in the Notes section.

## EXAMPLES

### Example 1: Output processes to a tree view

```PowerShell
PS C:\> Get-Process | Show-ObjectTree
```

This command gets the processes running on the local computer and sends them to a tree view window.

### Example 2: Save output to a variable, and then output a tree view

```PowerShell
PS C:\> ($A = Get-ChildItem -Path $pshome -Recurse) | sot
```

This command saves its output in a variable and sends it to **Show-ObjectTree**.

The command uses the Get-ChildItem cmdlet to get the files in the Windows PowerShell installation directory and its subdirectories.
The path to the installation directory is saved in the $pshome automatic variable.

The command uses the assignment operator (=) to save the output in the $A variable and the pipeline operator (|) to send the output to **Show-ObjectTree**.

The parentheses in the command establish the order of operations.
As a result, the output from the Get-ChildItem command is saved in the $A variable before it is sent to **Show-ObjectTree**.

## PARAMETERS

### -Filter
Pre-populates the Filter edit box, allowing filtering to be specified on the command line.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
Specifies that the cmdlet accepts input for **Show-ObjectTree**.

When you use the **InputObject** parameter to send a collection of objects to **Show-ObjectTree**, **Show-ObjectTree** treats the collection as one collection object, and it displays one row that represents the collection.

To display the each object in the collection, use a pipeline operator (|) to send objects to **Show-ObjectTree**.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Title
Specifies the text that appears in the title bar of the **Show-ObjectTree** window.

By default, the title bar displays the command that invokes **Show-ObjectTree**.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MinUi
If specified no window frame, filter box, or status bar will be displayed in the **Show-ObjectTree** window.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject

You can send any object to this cmdlet.

## OUTPUTS

### None

`Show-ObjectTree` does not output any objects.

## NOTES

* The command output that you send to **Show-ObjectTree** should not be formatted, such as by using the Format-Table or Format-Wide cmdlets. To select properties, use the Select-Object cmdlet.

* Deserialized output from remote commands might not be formatted correctly in the tree view window.

## RELATED LINKS

[Out-File](Out-File.md)

[Out-Printer](Out-Printer.md)

[Out-String](Out-String.md)
