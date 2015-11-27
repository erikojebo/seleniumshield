Write-Host "Compiling project..."

$ProjectFilePath = "$PSScriptRoot\..\src\SeleniumShield\SeleniumShield.csproj"
$DriverProjectFilePath = "$PSScriptRoot\..\src\SeleniumShield.Driver\SeleniumShield.Driver.csproj"

# Get the actual MSBuild path from the registry
$MSBuildToolsPathRegistryValue = Get-ItemProperty `
    -Path "HKLM:\SOFTWARE\Microsoft\MSBuild\ToolsVersions\14.0"  `
    -Name "MsBuildToolsPath"
$MSBuildDirectory = $MSBuildToolsPathRegistryValue.MSBuildToolsPath

& "$MSBuildDirectory\msbuild.exe" $ProjectFilePath '/t:Clean;Rebuild' /p:Configuration=Release
& "$MSBuildDirectory\msbuild.exe" $DriverProjectFilePath '/t:Clean;Rebuild' /p:Configuration=Release

Write-Host "Packing nuget package..."

nuget pack $ProjectFilePath -Prop Configuration=Release
nuget pack $DriverProjectFilePath -Prop Configuration=Release