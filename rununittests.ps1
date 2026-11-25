# Para instalar o Report Generator:
# To install the Report Generator:
# dotnet tool install -g dotnet-reportgenerator-globaltool

Remove-Item "./TestResults/" -Recurse -ErrorAction Ignore
Remove-Item "./global.json" -Recurse -ErrorAction Ignore
echo '{"test":{"runner":"Microsoft.Testing.Platform"}}' >> global.json
dotnet run --project ".\tests\Pororoca.Domain.Tests\Pororoca.Domain.Tests.csproj" -- --coverage --coverage-output-format cobertura --results-directory "./TestResults/"
reportgenerator "-reports:./TestResults/*.cobertura.xml" `
                "-targetdir:./TestResults/" `
                "-assemblyfilters:+Pororoca.Domain;+Pororoca.Domain.OpenAPI" `
                "-classfilters:-System.Threading.RateLimiting.*;-System.Collections.Generic.*;-Pororoca.Domain.Features.Common.PororocaLogger" `
                "-riskhotspotclassfilters:-System.Threading.RateLimiting.*;-System.Collections.Generic.*;-Pororoca.Domain.Features.Common.PororocaLogger" `
                "-filefilters:-*.g.cs" `
                "-reporttypes:Html"
Remove-Item "./global.json" -Recurse -ErrorAction Ignore