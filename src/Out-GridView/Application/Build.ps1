
dotnet publish
Import-Module .\bin\Debug\netcoreapp2.1\publish\OutGridView.Application.dll
Get-Process | Out-CrossGridView -PassThru
