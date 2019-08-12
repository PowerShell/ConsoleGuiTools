---
external help file: GraphicalToolsModule.dll-Help.xml
keywords: powershell,cmdlet
locale: en-us
Module Name: GraphicalTools
ms.date: 08/09/2019
schema: 2.0.0
title: Out-GridView
---

# Out-GridView

## SYNOPSIS
Sends output to an interactive table in a separate window.

## SYNTAX

### PassThru (Default)
```
Out-GridView [-InputObject <PSObject>] [-Title <String>] [-PassThru] [<CommonParameters>]
```

### Wait
```
Out-GridView [-InputObject <PSObject>] [-Title <String>] [-Wait] [<CommonParameters>]
```

### OutputMode
```
Out-GridView [-InputObject <PSObject>] [-Title <String>] [-OutputMode <OutputModeOption>] [<CommonParameters>]
```

## DESCRIPTION
The **Out-GridView** cmdlet sends the output from a command to a grid view window where the output is displayed in an interactive table.

You can use the following features of the table to examine your data:

- Hide, Show, and Reorder Columns: To hide, show, use the columns dropdown. Drag and drop column headers to reorder.
- Sort. To sort the data, click a column header. Click again to toggle from ascending to descending order.
- Quick Filter. Use the Filter box at the top of the window to search the text in the table. You can search for text in a particular column, search for literals, and search for multiple words.
- Column Filter. Use the Add Column Filter drop-down to create rules to filter the data. This is very useful for very large data sets, such as event logs.

For instructions for using these features, type `Get-Help Out-GridView -Full` and see How to Use the Grid View Window Features in the Notes section.

## EXAMPLES

### Example 1: Output processes to a grid view
```
PS C:\> Get-Process | Out-GridView
```

This command gets the processes running on the local computer and sends them to a grid view window.

### Example 2: Use a variable to output processes to a grid view
```
PS C:\> $P = Get-Process
PS C:\> $P | Out-GridView
```

This command also gets the processes running on the local computer and sends them to a grid view window.

The first command uses the Get-Process cmdlet to get the processes on the computer and then saves the process objects in the $P variable.

The second command uses a pipeline operator to send the $P variable to **Out-GridView**.

### Example 3: Display a formatted table in a grid view
```
PS C:\> Get-Process | Select-Object -Property Name, WorkingSet, PeakWorkingSet | Sort-Object -Property WorkingSet -Descending | Out-GridView
```

This command displays a formatted table in a grid view window.

It uses the Get-Process cmdlet to get the processes on the computer.

Then, it uses a pipeline operator (|) to send the process objects to the Select-Object cmdlet.
The command uses the **Property** parameter of **Select-Object** to select the Name, WorkingSet, and PeakWorkingSet properties to be displayed in the table.

Another pipeline operator sends the filtered objects to the Sort-Object cmdlet, which sorts them in descending order by the value of the **WorkingSet** property.

The final part of the command uses a pipeline operator (|) to send the formatted table to **Out-GridView**.

You can now use the features of the grid view to search, sort, and filter the data.

### Example 4: Save output to a variable, and then output a grid view
```
PS C:\> ($A = Get-ChildItem -Path $pshome -Recurse) | Out-GridView
```

This command saves its output in a variable and sends it to **Out-GridView**.

The command uses the Get-ChildItem cmdlet to get the files in the Windows PowerShell installation directory and its subdirectories.
The path to the installation directory is saved in the $pshome automatic variable.

The command uses the assignment operator (=) to save the output in the $A variable and the pipeline operator (|) to send the output to **Out-GridView**.

The parentheses in the command establish the order of operations.
As a result, the output from the Get-ChildItem command is saved in the $A variable before it is sent to **Out-GridView**.

### Example 5: Output processes for a specified computer to a grid view
```
PS C:\> Get-Process -ComputerName "Server01" | ogv -Title "Processes - Server01"
```

This command displays the processes that are running on the Server01 computer in a grid view window.

The command uses `ogv`, which is the built-in alias for the **Out-GridView** cmdlet, it uses the *Title* parameter to specify the window title.

### Example 6: Output data from remote computers to a grid view
```
PS C:\> Invoke-Command -ComputerName S1, S2, S3 -ScriptBlock {Get-Culture} | Out-GridView
```

This example shows the correct format for sending data collected from remote computers to the **Out-GridView** cmdlet.

The command uses the Invoke-Command cmdlet to run a Get-Culture command on three remote computers.
It uses a pipeline operator to send the data that is returned to the **Out-GridView** cmdlet.

Notice that the script block that contains the commands that are run remotely does not include the **Out-GridView** command.
If it did, the command would fail when it tried to open a grid view window on each of the remote computers.

### Example 7: Pass multiple items through Out-GridView
```
PS C:\> Get-Process | Out-GridView -PassThru | Export-Csv -Path .\ProcessLog.csv
```

This command lets you select multiple processes from the **Out-GridView** window.
The processes that you select are passed to the **Export-Csv** command and written to the ProcessLog.csv file.

The command uses the *PassThru* parameter of **Out-GridView**, which lets you send multiple items down the pipeline.
The *PassThru* parameter is equivalent to using the Multiple value of the *OutputMode* parameter.

### Example 8: Create a Windows shortcut to Out-GridView
```
PS C:\> Powershell.exe -Command "Get-Service | Out-GridView -Wait"
```

This command shows how to use the *Wait* parameter of **Out-GridView** to create a Windows shortcut to the **Out-GridView** window.
Without the *Wait* parameter, Windows PowerShell would exit as soon as the **Out-GridView** window opened, which would close the **Out-GridView** window almost immediately.

## PARAMETERS

### -InputObject
Specifies that the cmdlet accepts input for **Out-GridView**.

When you use the **InputObject** parameter to send a collection of objects to **Out-GridView**, **Out-GridView** treats the collection as one collection object, and it displays one row that represents the collection.
To display the each object in the collection, use a pipeline operator (|) to send objects to **Out-GridView**.

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
To send items from the interactive window down the pipeline, click to select the items and then click OK.

The values of this parameter determine how many items you can send down the pipeline.

- None.  No items. This is the default value.
- Single.  Zero items or one item. Use this value when the next command can take only one input object.
- Multiple.  Zero, one, or many items.  Use this value when the next command can take multiple input objects. This value is equivalent to the *Passthru* parameter.

This parameter was introduced in Windows PowerShell 3.0.

```yaml
Type: OutputModeOption
Parameter Sets: OutputMode
Aliases:
Accepted values: None, Single, Multiple

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Indicates that the cmdlet sends items from the interactive window down the pipeline as input to other commands.
By default, this cmdlet does not generate any output.
This parameter is equivalent to using the Multiple value of the *OutputMode* parameter.

To send items from the interactive window down the pipeline, click to select the items and then click OK.
Shift-click and Ctrl-click are supported.

This parameter was introduced in Windows PowerShell 3.0.

```yaml
Type: SwitchParameter
Parameter Sets: PassThru
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Title
Specifies the text that appears in the title bar of the **Out-GridView** window.

By default, the title bar displays the command that invokes **Out-GridView**.

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

### -Wait
Indicates that the cmdlet suppresses the command prompt and prevents Windows PowerShell from closing until the **Out-GridView** window is closed.
By default, the command prompt returns when the **Out-GridView** window opens.

This feature lets you use the **Out-GridView** cmdlets in Windows shortcuts.
When **Out-GridView** is used in a shortcut without the *Wait* parameter, the **Out-GridView** window appears only momentarily before Windows PowerShell closes.

This parameter was introduced in Windows PowerShell 3.0.

```yaml
Type: SwitchParameter
Parameter Sets: Wait
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
**Out-GridView** does not return any objects.

## NOTES
* You cannot use a remote command to open a grid view window on another computer.
* The command output that you send to **Out-GridView** cannot be formatted, such as by using the Format-Table or Format-Wide cmdlets. To select properties, use the Select-Object cmdlet.
* Deserialized output from remote commands might not be formatted correctly in the grid view window.
  How to Use the Grid View Window Features

  The following topics explain how to use the features of the window that **Out-GridView** displays.

  **How to Hide, Show, and Reorder Columns**

  **To hide or show a column:**

  1. Click on the Columns expander.

  2. In the Columns expander, toggle Columns that should appear.
    Only selected columns appear in the grid view window.

  **To reorder columns:**

  - Drag and drop the column into the desired location.

  **How to Sort Table Data**

  - To sort the data, click a column header.

  - To change the sort order, click the column header again.
Each time you click the same header, the sort order toggles between ascending to descending order.
The current order is indicated by a triangle in the column header.

  **How to Search in the Table (Quick Filter)**

  Use the Filter box to search for data in the table.
When you type in the box, only items that include the typed text appear in the table.

  - Search for text.
To search for text in the table, in the Filter box, type the text to find.

  - Search for multiple words.
To search for multiple words in the table, type the words separated by spaces.
**Out-GridView** displays rows that include all of the words (logical AND).

  - Search for literal phrases.
To search for phrases that include spaces or special characters, enclose the phrase in quotation marks.
**Out-GridView** displays rows that include an exact match for the phrase.

  **Use Column Filters to Filter the Table**

  You can use column filters to determine which items are displayed in the table.
  Items appear only when they satisfy all of the column filters that you establish.

  Each column filter has the following format:

  \<column\> \<operator\> \<value\>

  Column filters for different properties are connected by AND.
Column filters for the same property are connected by OR.
You cannot change the logical connectors.

  The column filters only affects the display.
It does not delete items from the table.

  **How to Add Column Filters**

  1. Click the Add Column Filters menu button.

  2. Click the column (property) you wish to add.

  **How to Edit a Column Filter**

  - To change an operator, click the operator drop down, and then click to select a different operator from the drop-down list.

  - To enter or change a value, type a value in the value box.

  - To create an OR statement, add a column filter with the same property.

  **How to Delete Column Filters**

  - To delete selected column filters, click the remove button beside each column filter.

  - To delete all column filters, click the Clear Filters button.

## RELATED LINKS

[Out-File](Out-File.md)

[Out-Printer](Out-Printer.md)

[Out-String](Out-String.md)
