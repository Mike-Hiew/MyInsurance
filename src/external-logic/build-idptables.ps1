# Build + test + package IdpTables for ODC. Run in PowerShell from E:\Hackaton\extension\basic_template
# (ASCII only on purpose: Windows PowerShell 5.1 misreads UTF-8 without BOM)
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
dotnet test .\IdpTables.Tests\IdpTables.Tests.csproj -c Release --logger "console;verbosity=normal"
if ($LASTEXITCODE -ne 0) { throw "Tests failed - not packaging" }
Remove-Item -Recurse -Force .\IdpTables\publish -ErrorAction SilentlyContinue
dotnet publish .\IdpTables\IdpTables.csproj -c Release -o .\IdpTables\publish
if ($LASTEXITCODE -ne 0) { throw "Publish failed" }
New-Item -ItemType Directory -Force .\dist | Out-Null
$zip = ".\dist\IdpTables.zip"
Remove-Item $zip -ErrorAction SilentlyContinue
Get-ChildItem .\IdpTables\publish -File | Where-Object { $_.Extension -ne ".pdb" } |
  Compress-Archive -DestinationPath $zip
Get-Item $zip | Select-Object FullName, Length
"OK - dist\IdpTables.zip"
