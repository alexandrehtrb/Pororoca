# Para instalar o Report Generator:
# To install the Report Generator:
# dotnet tool install -g dotnet-reportgenerator-globaltool

Remove-Item "./TestResults/" -Recurse -ErrorAction Ignore
# .NET SDK > 6.0.2xx on Windows 7 does not run unit tests for some reason,
# so we need to set the SDK version to below 6.0.2xx
# https://github.com/microsoft/vstest/issues/3543
$IsWindows7 = $IsWindows -and ((Get-ComputerInfo).OsName  -like "*Windows 7*")
if ($IsWindows7)
{
    Remove-Item ./global.json -Force -ErrorAction Ignore
    New-Item ./global.json
    Set-Content ./global.json '{"sdk":{"version":"6.0.102"}}'
}
dotnet test --collect:"XPlat Code Coverage" --results-directory "./TestResults/"
if ($IsWindows7)
{
    Remove-Item ./global.json -Force -ErrorAction Ignore
}
reportgenerator "-reports:./TestResults/**/coverage.cobertura.xml" "-targetdir:./TestResults/" -reporttypes:Html