param ( [string]$target )

# Test that we have allocated enough memory
$memoryMB = (Get-CimInstance win32_computersystem).TotalPhysicalMemory /1MB
$requiredMemoryMB = 2048
if($memoryMB -lt $requiredMemoryMB)
{
    throw "Building powershell requires at least $requiredMemoryMB MiB of memory and only $memoryMB MiB is present."
}

# Create the target directory. Delete if it already exists
if ( ! (test-path ${target} ) ) {
    new-item -type directory ${target}
}
else {
    if ( test-path -pathtype leaf ${target} ) {
        remove-item -force ${target}
        new-item -type directory ${target}
    }
}
push-location C:/GraphicalTools
Invoke-Build
Copy-Item -Verbose -Recurse "C:/PowerShellEditorServices/module" "${target}/PowerShellEditorServices"
