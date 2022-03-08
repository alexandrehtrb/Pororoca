# Stops the script run in case of any errors.
$ErrorActionPreference = "Stop"

Write-Host "Pororoca release maker." -ForegroundColor White

$now = (Get-Date -Format "dddd, dd/MM/yyyy HH:mm:ss K")
Write-Host $now -ForegroundColor DarkGray

[XML]$desktopCsprojXml = Get-Content ./src/Pororoca.Desktop/Pororoca.Desktop.csproj
$versionName = $desktopCsprojXml.Project.PropertyGroup.Version
Write-Host "Version name: ${versionName}" -ForegroundColor DarkGray

<#
$gitBranchName = (git branch --show-current)
if ($gitBranchName -ne "release_candidate")
{
	Write-Host "Releases need to be generated from the release_candidate branch." -ForegroundColor Red
	Pause
	Break
}
$gitStatusCheck = (git status --porcelain)
if ([string]::IsNullOrWhiteSpace($gitStatusCheck) -eq $false)
{
	Write-Host "There are uncommited changes in this repository. Commit ou clear them before release." -ForegroundColor Red
	Pause
	Break
}

git fetch
#>

$stopwatch = [System.Diagnostics.Stopwatch]::new()

Write-Host "Cleaning the solution..." -ForegroundColor DarkYellow
$stopwatch.Start()
dotnet clean --nologo --verbosity quiet
$stopwatch.Stop()
Write-Host "Solution cleaned ($($stopwatch.Elapsed.TotalSeconds.ToString("#"))s)." -ForegroundColor DarkGreen

Write-Host "Restoring the solution..." -ForegroundColor DarkYellow
$stopwatch.Restart()
dotnet restore --nologo --verbosity quiet
$stopwatch.Stop()
Write-Host "Solution restored ($($stopwatch.Elapsed.TotalSeconds.ToString("#"))s)." -ForegroundColor DarkGreen

Write-Host "Building the solution..." -ForegroundColor DarkYellow
$stopwatch.Restart()
dotnet build --configuration Release --nologo --verbosity quiet
$stopwatch.Stop()
Write-Host "Solution built ($($stopwatch.Elapsed.TotalSeconds.ToString("#"))s)." -ForegroundColor DarkGreen

Write-Host "Running unit tests..." -ForegroundColor DarkYellow
$stopwatch.Restart()
dotnet test --configuration Release --nologo --verbosity quiet
$stopwatch.Stop()
Write-Host "Solution tests run ($($stopwatch.Elapsed.TotalSeconds.ToString("#"))s)." -ForegroundColor DarkGreen

Write-Host "Deleting 'out' folder..." -ForegroundColor DarkYellow
$stopwatch.Restart()
Remove-Item "./out/" -Recurse -ErrorAction Ignore
$stopwatch.Stop()
Write-Host "Output folder deleted ($($stopwatch.Elapsed.Seconds)s)." -ForegroundColor DarkGreen

$runtimes = @(`
#'linux-x64' ` # Linux and Mac OS releases should be built on one of those OSs, because of chmod and zip
#,'linux-arm64' `
#,'osx-x64' `
#,'osx-arm64' `
'win7-x64' ` # Windows releases need to be built on a Windows machine, because of dotnet
,'win7-x86' `
,'win-x64' `
,'win-x86' `
,'win-arm' `
,'win-arm64' `
)

foreach ($runtime in $runtimes)
{
	if ($runtime -like "*win7*")
	{
		$publishSingleFile = $False
	}
	else
	{
		$publishSingleFile = $True
	}

	$fullAppReleaseName = "Pororoca_${versionName}_${runtime}"
	$outputFolder = "./out/${fullAppReleaseName}"
	$zipName = "${fullAppReleaseName}.zip"
	$publishSingleFileArg = $(${publishSingleFile}.ToString().ToLower())
	
	Write-Host "Publishing for ${runtime}..." -ForegroundColor DarkYellow
	
	$stopwatch.Restart()
	
	dotnet publish ./src/Pororoca.Desktop/Pororoca.Desktop.csproj `
		--verbosity quiet `
		--nologo `
		--configuration Release `
		-p:PublishSingleFile=${publishSingleFileArg} `
		--self-contained true `
		--runtime $runtime `
		--output $outputFolder
	
	if ($runtime -like "*win*")
	{
		Rename-Item -Path "${outputFolder}/Pororoca.Desktop.exe" -NewName "Pororoca.exe"
	}
	else
	{
		Rename-Item "${outputFolder}/Pororoca.Desktop" -NewName "Pororoca"
		
		# If this is script is running on Linux or Mac OS,
		# lets also set UNIX executable attribute for the program,
		# if the target runtime is Linux or Mac OS
		if ($IsLinux -or $IsMacOS)
		{
			chmod +x "${outputFolder}/Pororoca"
		}
		else
		{
			Write-Host "Program file will not have UNIX executable attribute.`nRemember to run 'chmod +x' before running." -ForegroundColor Magenta
		}
	}

	if ($runtime -like "*osx*")
	{
		Write-Host "Remember to update Info.plist file with the correct version!" -ForegroundColor Magenta
		Write-Host "Creating custom folder structure for Mac OSX .app..." -ForegroundColor DarkYellow
		[void](mkdir "${outputFolder}/Pororoca.app")
		[void](mkdir "${outputFolder}/Pororoca.app/Contents")
		[void](mkdir "${outputFolder}/Pororoca.app/Contents/MacOS")
		[void](mkdir "${outputFolder}/Pororoca.app/Contents/Resources")
		Copy-Item -Path "./src/Pororoca.Desktop/Assets/MacOSX/Info.plist" `
				  -Destination "${outputFolder}/Pororoca.app/Contents/"
		Copy-Item -Path "./src/Pororoca.Desktop/Assets/MacOSX/pororoca.icns" `
				  -Destination "${outputFolder}/Pororoca.app/Contents/Resources/"
		Get-ChildItem $outputFolder -File | Copy-Item -Destination "${outputFolder}/Pororoca.app/Contents/MacOS/" -Force
		Get-ChildItem $outputFolder -File | Remove-Item
	}

	if ($runtime -like "*linux*")
	{
		# Copy logo for users to create launchers
		Copy-Item -Path "./pororoca.png" `
			  -Destination $outputFolder
	}

	Copy-Item -Path "./LICENCE.md" `
			  -Destination $outputFolder


	if ($IsWindows)
	{
		Compress-Archive `
			-CompressionLevel Optimal `
			-Path $outputFolder `
			-DestinationPath "./out/${zipName}"	
	}
	else
	{
		cd $outputFolder
		zip -9 -r ../$zipName *
		cd ../..
	}

	Remove-Item $outputFolder -Force -Recurse -ErrorAction Ignore
	
	$stopwatch.Stop()
	Write-Host "Package created on ./out/${zipName} ($($stopwatch.Elapsed.TotalSeconds.ToString("#"))s)." -ForegroundColor DarkGreen

	Write-Host "SHA256 hash for package ${runtime}: $((Get-FileHash ./out/${zipName} -Algorithm SHA256).Hash)" -ForegroundColor DarkGreen

}

Write-Host "Finished!" -ForegroundColor DarkGreen
