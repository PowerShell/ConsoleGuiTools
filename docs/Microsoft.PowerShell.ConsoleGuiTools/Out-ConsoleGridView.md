---
external help file: ConsoleGuiToolsModule.dll-Help.xml
keywords: powershell,cmdlet
locale: en-us
Module Name: Microsoft.PowerShell.ConsoleGuiTools
ms.date: 08/09/2019
schema: 2.0.0
title: Out-ConsoleGridView
---

# Out-ConsoleGridView

## SYNOPSIS

Sends output to an interactive table in the same console window.

## SYNTAX

```PowerShell
 Out-ConsoleGridView [-InputObject <psobject>] [-Title <string>] [-OutputMode {None | Single |
    Multiple}] [-Filter <string>] [<CommonParameters>]
```

## DESCRIPTION

The **Out-ConsoleGridView** cmdlet sends the output from a command to a grid view window where the output is displayed in an interactive table.

You can use the following features of the table to examine your data:

- Quick Filter. Use the Filter box at the top of the window to search the text in the table. You can search for text in a particular column, search for literals, and search for multiple words. You can use the `-Filter` command to pre-populate the Filter box.

For instructions for using these features, type `Get-Help Out-ConsoleGridView -Full` and see How to Use the Grid View Window Features in the Notes section.

To send items from the interactive window down the pipeline, click to select the items (either the the mouse in terminals that support mouse or the `SPACE` key) and then press `ENTER`. `ESC` cancels.

## EXAMPLES

### Example 1: Output processes to a grid view

```PowerShell
PS C:\> Get-Process | Out-ConsoleGridView
```

This command gets the processes running on the local computer and sends them to a grid view window.

### Example 2: Use a variable to output processes to a grid view

```PowerShell
PS C:\> $P = Get-Process
PS C:\> $P | Out-ConsoleGridView -OutputMode Single
```

This command also gets the processes running on the local computer and sends them to a grid view window.

The first command uses the Get-Process cmdlet to get the processes on the computer and then saves the process objects in the $P variable.

The second command uses a pipeline operator to send the $P variable to **Out-ConsoleGridView**.

By specifying `-OutputMode Single` the grid view window will be restricted to a single selection, ensuring now more than a single object is returned.

### Example 3: Display a formatted table in a grid view

```PowerShell
PS C:\> Get-Process | Select-Object -Property Name, WorkingSet, PeakWorkingSet | Sort-Object -Property WorkingSet -Descending | Out-ConsoleGridView
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
PS C:\> ($A = Get-ChildItem -Path $pshome -Recurse) | Out-ConsoleGridView
```

This command saves its output in a variable and sends it to **Out-ConsoleGridView**.

The command uses the Get-ChildItem cmdlet to get the files in the Windows PowerShell installation directory and its subdirectories.
The path to the installation directory is saved in the $pshome automatic variable.

The command uses the assignment operator (=) to save the output in the $A variable and the pipeline operator (|) to send the output to **Out-ConsoleGridView**.

The parentheses in the command establish the order of operations.
As a result, the output from the Get-ChildItem command is saved in the $A variable before it is sent to **Out-ConsoleGridView**.

### Example 5: Output processes for a specified computer to a grid view

```PowerShell
PS C:\> Get-Process -ComputerName "Server01" | ocgv -Title "Processes - Server01"
```

This command displays the processes that are running on the Server01 computer in a grid view window.

The command uses `ocgv`, which is the built-in alias for the **Out-ConsoleGridView** cmdlet, it uses the *Title* parameter to specify the window title.

### Example 6: Define a function to kill processes using a graphical chooser

```PowerShell
PS C:\> function killp { Get-Process | Out-ConsoleGridView -OutputMode Single -Filter $args[0] | Stop-Process -Id {$_.Id} }
PS C:\> killp note
```
This example shows defining a function named `killp` that shows a grid view of all running processes and allows the user to select one to kill it.

The example uses the `-Filter` paramter to filter for all proceses with a name that includes `note` (thus highlighting `Notepad` if it were running. Selecting an item in the grid view and pressing `ENTER` will kill that process. 

### Example 7: Pass multiple items through Out-ConsoleGridView

```PowerShell
PS C:\> Get-Process | Out-ConsoleGridView -PassThru | Export-Csv -Path .\ProcessLog.csv
```

This command lets you select multiple processes from the **Out-ConsoleGridView** window.
The processes that you select are passed to the **Export-Csv** command and written to the ProcessLog.csv file.

The command uses the *PassThru* parameter of **Out-ConsoleGridView**, which lets you send multiple items down the pipeline.
The *PassThru* parameter is equivalent to using the Multiple value of the *OutputMode* parameter.

### Example 8: Use F7 as "Show Command History"

Save See [this gist](https://gist.github.com/tig/cbbeab7f53efd73e329afd6d4b838191) as `F7History.ps1` and run `F7History.ps1` in your `$profile`.

Press `F7` to see the history for the current PowerShell instance

Press `Shift-F7` to see the history for all PowerShell instances.

Whatever you select within `Out-ConsoleGridView` will be inserted on your command line. 

Whatever was typed on the command line prior to hitting `F7` or `Shift-F7` will be used as a filter.

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
Specifies that the cmdlet accepts input for **Out-ConsoleGridView**.

When you use the **InputObject** parameter to send a collection of objects to **Out-ConsoleGridView**, **Out-ConsoleGridView** treats the collection as one collection object, and it displays one row that represents the collection.

To display the each object in the collection, use a pipeline operator (|) to send objects to **Out-ConsoleGridView**.

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

### -OutputMode
Specifies the items that the interactive window sends down the pipeline as input to other commands.
By default, this cmdlet does not generate any output.

To send items from the interactive window down the pipeline, click to select the items (either the the mouse in terminals that support mouse or the `SPACE` key) and then press `ENTER`. `ESC` cancels.

The values of this parameter determine how many items you can send down the pipeline.

- None. No items. 
- Single.  Zero items or one item. Use this value when the next command can take only one input object.
- Multiple.  Zero, one, or many items.  Use this value when the next command can take multiple input objects. This is the default value.

```yaml
Type: OutputModeOption
Parameter Sets: OutputMode
Aliases:
Accepted values: None, Single, Multiple

Required: False
Position: Named
Default value: Multiple
Accept pipeline input: False
Accept wildcard characters: False
```

### -Title
Specifies the text that appears in the title bar of the **Out-ConsoleGridView** window.

By default, the title bar displays the command that invokes **Out-ConsoleGridView**.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject

You can send any object to this cmdlet.

## OUTPUTS

### System.Object

By default `Out-ConsoleGridView` returns objects representing the selected rows to the pipeline. Use `-OutputMode` to change this behavior.

## NOTES

* The command output that you send to **Out-ConsoleGridView** should not be formatted, such as by using the Format-Table or Format-Wide cmdlets. To select properties, use the Select-Object cmdlet.

* Deserialized output from remote commands might not be formatted correctly in the grid view window.

## RELATED LINKS

[Out-GridView](Out-GridView.md)

[Out-File](Out-File.md)

[Out-Printer](Out-Printer.md)

[Out-String](Out-String.md)
