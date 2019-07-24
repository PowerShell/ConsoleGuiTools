
dotnet publish
Import-Module .\bin\Debug\netcoreapp2.1\publish\OutGridView.dll
Get-Process | Out-CrossGridView 
