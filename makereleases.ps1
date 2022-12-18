# Stops the script run in case of any errors.
$ErrorActionPreference = "Stop"

########################## Pipeline #############################

function Run-Pipeline
{
	$runtimes = Get-RuntimesToPublishFor
	$now = (Get-Date -Format "dddd, dd/MM/yyyy HH:mm:ss K")
	$versionName = Read-VersionName
	$sw = [System.Diagnostics.Stopwatch]::new()

	Show-Greeting -VersionName $versionName -Now $now
	Clean-Solution -Stopwatch $sw
	Restore-Solution -Stopwatch $sw
	Build-Solution -Stopwatch $sw
	Run-UnitTests -Stopwatch $sw
	Clean-OutputFolder -Stopwatch $sw

	Generate-PororocaTestRelease -Stopwatch $sw
	foreach ($runtime in $runtimes)
	{
		Generate-PororocaDesktopRelease -Stopwatch $sw -VersionName $versionName -Runtime $runtime
	}
	Show-Goodbye
}

function Get-RuntimesToPublishFor
{
	# Dropping support for arm releases, starting at version 1.6.0
	# This is because we are using AvaloniaEdit.TextMate and TextMateSharp,
	# which rely on native C dlls;
	# No current support for arm yet (2022-11-20)
	# osx-arm64 is now supported
	$unixRuntimes = @(`
		'linux-x64' ` 
		#,'linux-arm64' `
		,'osx-x64' `
		,'osx-arm64' `
	)
	$windowsRuntimes = @(`
		#'win7-x64' ` 
		#,'win7-x86' `
		'win-x64_portable' `
		,'win-x86_portable' `
		#,'win-arm_portable' `
		#,'win-arm64_portable' `

		,'win-x64_installer' `
		,'win-x86_installer' `
		#,'win-arm_installer' `
		#,'win-arm64_installer' `
	)

	# Windows releases should be built on a Windows machine, because of dotnet
	# Linux and Mac OS releases should be built on one of those OSs, because of chmod and zip
	#return $IsWindows ? $windowsRuntimes : $unixRuntimes
	return @("linux-x64")
}

#################### Pre-release build and tests ####################

function Show-Greeting
{
    param (
		[string]$versionName,
		[string]$now
    )
	Write-Host "Pororoca release maker." -ForegroundColor White
	Write-Host $now -ForegroundColor DarkGray
	Write-Host "Version name: ${versionName}" -ForegroundColor DarkGray
}

function Show-Goodbye
{
	Write-Host "Finished!" -ForegroundColor DarkGreen
}

function Clean-Solution
{
	param (
        [System.Diagnostics.Stopwatch]$stopwatch
    )

	Write-Host "Cleaning the solution..." -ForegroundColor DarkYellow
	$stopwatch.Restart()
	dotnet clean --nologo --verbosity quiet
	$stopwatch.Stop()
	Write-Host "Solution cleaned ($($stopwatch.Elapsed.TotalSeconds.ToString("#"))s)." -ForegroundColor DarkGreen
}

function Restore-Solution
{
	param (
        [System.Diagnostics.Stopwatch]$stopwatch
    )

	Write-Host "Restoring the solution..." -ForegroundColor DarkYellow
	$stopwatch.Restart()
	dotnet restore --nologo --verbosity quiet
	$stopwatch.Stop()
	Write-Host "Solution restored ($($stopwatch.Elapsed.TotalSeconds.ToString("#"))s)." -ForegroundColor DarkGreen
}

function Build-Solution
{
	param (
        [System.Diagnostics.Stopwatch]$stopwatch
    )

	Write-Host "Building the solution..." -ForegroundColor DarkYellow
	$stopwatch.Restart()
	dotnet build --configuration Release --nologo --verbosity quiet
	$stopwatch.Stop()
	Write-Host "Solution built ($($stopwatch.Elapsed.TotalSeconds.ToString("#"))s)." -ForegroundColor DarkGreen
}

function Run-UnitTests
{
	param (
        [System.Diagnostics.Stopwatch]$stopwatch
    )

	Write-Host "Running unit tests..." -ForegroundColor DarkYellow
	$stopwatch.Restart()
	dotnet test --configuration Release --nologo --verbosity quiet
	$stopwatch.Stop()
	Write-Host "Solution tests run ($($stopwatch.Elapsed.TotalSeconds.ToString("#"))s)." -ForegroundColor DarkGreen
}

function Clean-OutputFolder
{
	param (
        [System.Diagnostics.Stopwatch]$stopwatch
    )

	Write-Host "Deleting 'out' folder..." -ForegroundColor DarkYellow
	$stopwatch.Restart()
	Remove-Item "./out/" -Recurse -ErrorAction Ignore
	$stopwatch.Stop()
	Write-Host "Output folder deleted ($($stopwatch.Elapsed.Seconds)s)." -ForegroundColor DarkGreen
}

#################### Release generation ####################

function Read-VersionName
{
	[XML]$desktopCsprojXml = Get-Content ./src/Pororoca.Desktop/Pororoca.Desktop.csproj
	$versionName = $desktopCsprojXml.Project.PropertyGroup.Version
	return $versionName
}

function Generate-PororocaTestRelease {
    param (
        [System.Diagnostics.Stopwatch]$stopwatch
    )

	Write-Host "Publishing Pororoca.Test library..." -ForegroundColor DarkYellow

	dotnet pack ./src/Pororoca.Test/Pororoca.Test.csproj `
		--nologo `
		--verbosity quiet `
		--configuration Release

	Write-Host "Package created! ($($stopwatch.Elapsed.Seconds)s)." -ForegroundColor DarkGreen
}

function Generate-PororocaDesktopRelease {
    param (
        [System.Diagnostics.Stopwatch]$stopwatch,
		[string]$versionName,
		[string]$runtime
    )

	$fullAppReleaseName = "Pororoca_${versionName}_${runtime}"
	$outputFolder = "./out/${fullAppReleaseName}"
	$zipName = "${fullAppReleaseName}.zip"
	$isInstallOnWindowsRelease = ($runtime -like "*_installer")
	$dotnetPublishRuntime = $runtime.Replace("_installer","").Replace("_portable","")
	
	Write-Host "Publishing Pororoca.Desktop for ${runtime}..." -ForegroundColor DarkYellow

	$stopwatch.Restart()
	
	Publish-PororocaDesktop -Runtime $dotnetPublishRuntime -IsInstallOnWindowsRelease $isInstallOnWindowsRelease -OutputFolder $outputFolder
	Rename-Executable -Runtime $runtime -OutputFolder $outputFolder
	Set-ExecutableAttributesIfUnix -Runtime $runtime -OutputFolder $outputFolder
	Make-AppFolderIfMacOS -Runtime $runtime -OutputFolder $outputFolder
	Copy-LogoIfLinux -Runtime $runtime -OutputFolder $outputFolder
	Copy-Licence -OutputFolder $outputFolder
	if ($isInstallOnWindowsRelease)
	{
		Copy-IconIfInstalledOnWindows -Runtime $runtime -OutputFolder $outputFolder
		Write-Host "Generating Windows installer for ${runtime}..." -ForegroundColor DarkYellow
		Pack-ReleaseInWindowsInstaller -GeneralOutFolder ".\out" -InstallerFilesFolder $outputFolder -InstallerFileName "${fullAppReleaseName}.exe" -VersionName $versionName
		$stopwatch.Stop()
		Write-Host "Windows installer for ${dotnetPublishRuntime} created: ./out/${fullAppReleaseName}.exe ($($stopwatch.Elapsed.TotalSeconds.ToString("#"))s)." -ForegroundColor DarkGreen
	}
	else
	{
		Compress-Package -OutputFolder $outputFolder -ZipName $zipName
		$stopwatch.Stop()
		Write-Host "Package created on ./out/${zipName} ($($stopwatch.Elapsed.TotalSeconds.ToString("#"))s)." -ForegroundColor DarkGreen
		Write-Host "SHA256 hash for package ${runtime}: $((Get-FileHash ./out/${zipName} -Algorithm SHA256).Hash)" -ForegroundColor DarkGreen
	}
	
}

function Publish-PororocaDesktop
{
	param (
		[string]$runtime,
		[string]$outputFolder,
		[bool]$isInstallOnWindowsRelease = $false
    )

	if ($runtime -like "*win*")
	{
		# .NET SDK 6.0.3xx and greater allows for single file publishing for Windows 7
		# for HTTP/3 to work, we cannot ship as single-file application,
		# unless, and only for Windows, if we include msquic.dll next to the generated .exe file
		# https://github.com/dotnet/runtime/issues/79727
		$publishSingleFile = $True #$False
	}
	else
	{
		$publishSingleFile = $False
	}

	$publishSingleFileArg = $(${publishSingleFile}.ToString().ToLower())
	$isInstallOnWindowsReleaseArg = $(${isInstallOnWindowsRelease}.ToString().ToLower())
	
	dotnet publish ./src/Pororoca.Desktop/Pororoca.Desktop.csproj `
		--verbosity quiet `
		--nologo `
		--configuration Release `
		-p:PublishSingleFile=${publishSingleFileArg} `
		-p:PublishForInstallOnWindows=${isInstallOnWindowsReleaseArg} `
		--self-contained true `
		--runtime $runtime `
		--output $outputFolder
	
	if ($runtime -like "*win*")
	{
		# let's copy the msquic.dll file next to the generated .exe
		Copy-Item -Path "./src/Pororoca.Desktop/bin/Release/${runtime}/msquic.dll" `
			  	  -Destination $outputFolder
	}
}

function Rename-Executable
{
	param (
		[string]$runtime,
		[string]$outputFolder
    )

	if ($runtime -like "*win*")
	{
		Rename-Item -Path "${outputFolder}/Pororoca.Desktop.exe" -NewName "Pororoca.exe"
	}
	else
	{
		Rename-Item "${outputFolder}/Pororoca.Desktop" -NewName "Pororoca"
	}
}


function Set-ExecutableAttributesIfUnix
{
	param (
		[string]$runtime,
		[string]$outputFolder
    )

	if ($runtime -notlike "*win*")
	{
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
}

function Make-AppFolderIfMacOS
{
	param (
		[string]$runtime,
		[string]$outputFolder
    )

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
}

function Copy-LogoIfLinux
{
	param (
		[string]$runtime,
		[string]$outputFolder
    )

	if ($runtime -like "*linux*")
	{
		# Copy logo for users to create launchers
		Copy-Item -Path "./pororoca.png" `
			  -Destination $outputFolder
	}
}

function Copy-IconIfInstalledOnWindows
{
	param (
		[string]$runtime,
		[string]$outputFolder
    )

	if (($runtime -like "*win*") -and ($runtime -like "*_installer*"))
	{
		# Copy icon for windows installer
		Copy-Item -Path "./src/Pororoca.Desktop/Assets/pororoca_icon.ico" `
			  -Destination $outputFolder
	}
}

function Copy-Licence
{
	param (
		[string]$outputFolder
    )

	Copy-Item -Path "./LICENCE.md" `
			  -Destination $outputFolder
}

function Compress-Package
{
	param (
		[string]$outputFolder,
		[string]$zipName
    )

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
}

function Pack-ReleaseInWindowsInstaller
{
	param (
		[string]$generalOutFolder, # the "./out/" folder
		[string]$installerFilesFolder, # the "./out/Pororoca_x.y.z_win-x64_installer" folder
		[string]$installerFileName,
		[string]$versionName
    )

	$installerOutFileAbsolutePath = $((Resolve-Path $generalOutFolder).ToString()) + "\" + $installerFileName
	$installerFilesDirAbsolutePath = $((Resolve-Path $installerFilesFolder).ToString())

	# makensis must be added to PATH
	# -WX ` # treat warnings as errors
	# -V2 ` # verbosity no info
	makensis -WX -V2 "/XOutFile ${installerOutFileAbsolutePath}" `
		"/DSHORT_VERSION=${versionName}" `
		"/DINPUT_FILES_DIR=${installerFilesDirAbsolutePath}" `
		.\src\Pororoca.WindowsInstaller\Installer.nsi

	Remove-Item $installerFilesFolder -Force -Recurse -ErrorAction Ignore
}

########################## Execute #############################

Run-Pipeline