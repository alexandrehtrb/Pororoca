# Para instalar o Report Generator:
# To install the Report Generator:
# dotnet tool install -g dotnet-reportgenerator-globaltool

Remove-Item "./TestResults/" -Recurse -ErrorAction Ignore
dotnet test --collect:"XPlat Code Coverage" --results-directory "./TestResults/" --filter FullyQualifiedName!~Pororoca.Test.Tests
reportgenerator "-reports:./TestResults/**/coverage.cobertura.xml" `
                "-targetdir:./TestResults/" `
                "-assemblyfilters:+Pororoca.Domain;+Pororoca.Domain.OpenAPI" `
                "-classfilters:-System.Threading.RateLimiting.*;-System.Collections.Generic.*" `
                "-riskhotspotclassfilters:-System.Threading.RateLimiting.*;-System.Collections.Generic.*" `
                "-filefilters:-*.g.cs" `
                "-reporttypes:Html"