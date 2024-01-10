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
	Audit-Solution -Stopwatch $sw
	Build-Solution -Stopwatch $sw
	Run-UnitTests -Stopwatch $sw
	Clean-OutputFolder -Stopwatch $sw

	Generate-PororocaTestRelease -Stopwatch $sw -VersionName $versionName
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
	# osx-arm64 is now supported (2022-11-30), thanks to AvaloniaEdit version 11.0.0-preview2

	# osx-arm64 doesn't work for some reason,
	# but osx-x64 works on Mac OSes with Apple Silicon (arm64)
	$unixRuntimes = @(`
		'linux-x64' ` 
		,'debian-x64' ` 
		#,'linux-arm64' `
		,'osx-x64' `
		#,'osx-arm64' `
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
	return @("debian-x64")
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

function Audit-Solution
{
	param (
        [System.Diagnostics.Stopwatch]$stopwatch
    )

	Write-Host "Auditing the solution..." -ForegroundColor DarkYellow
	$stopwatch.Restart()
	$jsonObj = (dotnet list package --vulnerable --include-transitive --format json) | ConvertFrom-Json;
	$stopwatch.Stop()
	$hasAnyVulnerability = $false;
	foreach ($project in $jsonObj.projects)
	{
		if ($project.frameworks -ne $null) { $hasAnyVulnerability = $true; Break; }
	}
	if ($hasAnyVulnerability)
	{
		Write-Host "Vulnerabilities found ($($stopwatch.Elapsed.TotalSeconds.ToString("#"))s)." -ForegroundColor Magenta
		Write-Host "They can be ignored if they are only in test projects." -ForegroundColor DarkGray
		dotnet list package --vulnerable --include-transitive
	}
	else
	{
		Write-Host "No vulnerabilities found ($($stopwatch.Elapsed.TotalSeconds.ToString("#"))s)." -ForegroundColor DarkGreen
	}
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
	dotnet test --configuration Release --nologo --verbosity quiet --filter FullyQualifiedName!~Pororoca.Test.Tests
	$stopwatch.Stop()
	Write-Host "Solution tests run ($($stopwatch.Elapsed.TotalSeconds.ToString("#"))s)." -ForegroundColor DarkGreen
}

function Clean-OutputFolder
{
	param (
        [System.Diagnostics.Stopwatch]$stopwatch
    )

	Write-Host "Cleaning 'out' folder..." -ForegroundColor DarkYellow
	$stopwatch.Restart()
	[void](Remove-Item "./out/" -Recurse -ErrorAction Ignore)
	[void](mkdir "out")
	$stopwatch.Stop()
	Write-Host "Output folder cleaned ($($stopwatch.Elapsed.Seconds)s)." -ForegroundColor DarkGreen
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
        [System.Diagnostics.Stopwatch]$stopwatch,
        [string]$versionName
    )

	Write-Host "Publishing Pororoca.Test library..." -ForegroundColor DarkYellow

	dotnet pack ./src/Pororoca.Test/Pororoca.Test.csproj `
		--nologo `
		--verbosity quiet `
		--configuration Release
	
	Copy-Item -Path "./src/Pororoca.Test/bin/Release/Pororoca.Test.${versionName}.nupkg" `
	          -Destination "./out/Pororoca.Test.${versionName}.nupkg"

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
	$isDebianDpkgRelease = ($runtime -like "debian*")
	$dotnetPublishRuntime = $runtime.Replace("_installer","").Replace("_portable","").Replace("debian","linux")
	
	Write-Host "Publishing Pororoca.Desktop for ${runtime}..." -ForegroundColor DarkYellow

	$stopwatch.Restart()
	
	Publish-PororocaDesktop -Runtime $dotnetPublishRuntime -IsInstallOnWindowsRelease $isInstallOnWindowsRelease -IsInstallOnDebianRelease $isDebianDpkgRelease -OutputFolder $outputFolder
	Rename-Executable -Runtime $runtime -OutputFolder $outputFolder
	Set-ExecutableAttributesIfUnix -Runtime $runtime -OutputFolder $outputFolder
	Make-AppFolderIfMacOS -Runtime $runtime -OutputFolder $outputFolder
	Copy-LogoIfLinux -Runtime $runtime -OutputFolder $outputFolder
	Copy-Licence -OutputFolder $outputFolder
	Generate-SBOM -OutputFolder $outputFolder
	if ($isInstallOnWindowsRelease)
	{
		Copy-IconIfInstalledOnWindows -Runtime $runtime -OutputFolder $outputFolder
		Write-Host "Generating Windows installer for ${runtime}..." -ForegroundColor DarkYellow
		Pack-ReleaseInWindowsInstaller -GeneralOutFolder ".\out" -InstallerFilesFolder $outputFolder -InstallerFileName "${fullAppReleaseName}.exe" -VersionName $versionName
		$stopwatch.Stop()
		Write-Host "Windows installer for ${dotnetPublishRuntime} created: ./out/${fullAppReleaseName}.exe ($($stopwatch.Elapsed.TotalSeconds.ToString("#"))s)." -ForegroundColor DarkGreen
	}
	elseif ($isDebianDpkgRelease)
	{
		Write-Host "Generating Debian package for ${runtime}..." -ForegroundColor DarkYellow
		Pack-ReleaseInDebianDpkg -GeneralOutFolder "./out" -InstallerFilesFolder $outputFolder -InstallerFileName "${fullAppReleaseName}.dpkg" -VersionName $versionName
		$stopwatch.Stop()
		Write-Host "Debian package for ${dotnetPublishRuntime} created: ./out/${fullAppReleaseName}.deb ($($stopwatch.Elapsed.TotalSeconds.ToString("#"))s)." -ForegroundColor DarkGreen
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
		[bool]$isInstallOnWindowsRelease = $false,
		[bool]$isInstallOnDebianRelease = $false
    )

	if (($runtime -like "*win*") -or ($runtime -like "*linux*"))
	{
		# .NET SDK 6.0.3xx and greater allows for single file publishing for Windows 7
		# for HTTP/3 to work, we cannot ship as single-file application,
		# unless, and only for Windows, if we include msquic.dll next to the generated .exe file
		# https://github.com/dotnet/runtime/issues/79727
		# update (2023-12-11): let's try single-file publishing for Linux too
		$publishSingleFile = $True #$False
	}
	else
	{
		$publishSingleFile = $False
	}

	$publishSingleFileArg = $(${publishSingleFile}.ToString().ToLower())
	$isInstallOnWindowsReleaseArg = $(${isInstallOnWindowsRelease}.ToString().ToLower())
	$isInstallOnDebianReleaseArg = $(${isInstallOnDebianRelease}.ToString().ToLower())
	
	# set UITestsEnabled to false to hide 'Run UI tests'
	dotnet publish ./src/Pororoca.Desktop/Pororoca.Desktop.csproj `
		--verbosity quiet `
		--nologo `
		--configuration Release `
		-p:PublishSingleFile=${publishSingleFileArg} `
		-p:PublishForInstallOnWindows=${isInstallOnWindowsReleaseArg} `
		-p:PublishForInstallOnDebian=${isInstallOnDebianReleaseArg} `
		-p:UITestsEnabled=false `
		--self-contained true `
		--runtime $runtime `
		--output $outputFolder
	
	if ($runtime -like "*win*")
	{
		# let's copy the msquic.dll file next to the generated .exe
		Copy-Item -Path "./src/Pororoca.Desktop/bin/Release/net8.0/${runtime}/msquic.dll" `
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
		Copy-Item -Path "./src/Pororoca.Desktop.MacOSX/Info.plist" `
				  -Destination "${outputFolder}/Pororoca.app/Contents/"
		Copy-Item -Path "./src/Pororoca.Desktop.MacOSX/pororoca.icns" `
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

function Generate-SBOM
{
	param (
		[string]$outputFolder,
		[string]$versionName
    )

	dotnet CycloneDX ./src/Pororoca.Desktop/Pororoca.Desktop.csproj -o $outputFolder -f sbom.json -sv $versionName --json
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
		.\src\Pororoca.Desktop.WindowsInstaller\Installer.nsi

	Remove-Item $installerFilesFolder -Force -Recurse -ErrorAction Ignore
}

function Pack-ReleaseInDebianDpkg
{
	param (
		[string]$generalOutFolder, # the "./out" folder
		[string]$installerFilesFolder, # the "./out/Pororoca_x.y.z_debian-x64/" folder
		[string]$installerFileName,
		[string]$versionName
    )
	# https://wiki.freepascal.org/Debian_package_structure
	# https://martin.hoppenheit.info/blog/2016/where-to-put-application-icons-on-linux/
	
	[void](mkdir "${generalOutFolder}/deb")
	# Debian control file
	[void](mkdir "${generalOutFolder}/deb/DEBIAN")
	Copy-Item -Path "./src/Pororoca.Desktop.Debian/control" -Destination "${generalOutFolder}/deb/DEBIAN"
	# Executable file
	[void](mkdir "${generalOutFolder}/deb/usr")
	[void](mkdir "${generalOutFolder}/deb/usr/bin")
	Copy-Item -Path "./${installerFilesFolder}/Pororoca" -Destination "${generalOutFolder}/deb/usr/bin/pororoca"
	# Shared libraries
	# chmod 644 --> set read-only attributes 
	[void](mkdir "${generalOutFolder}/deb/usr/lib")
	[void](mkdir "${generalOutFolder}/deb/usr/lib/pororoca")
	Get-ChildItem $installerFilesFolder -File -Filter "*.so" | Copy-Item -Destination "${generalOutFolder}/deb/usr/lib/pororoca" -Force
	Get-ChildItem "${generalOutFolder}/deb/usr/lib/pororoca" -File -Filter "*.so" | % { chmod 644 $_.FullName }
	# Desktop shortcut
	[void](mkdir "${generalOutFolder}/deb/usr/share")
	[void](mkdir "${generalOutFolder}/deb/usr/share/applications")
	Copy-Item -Path "./src/Pororoca.Desktop.Debian/Pororoca.desktop" -Destination "${generalOutFolder}/deb/usr/share/applications/Pororoca.desktop"
	# Desktop icon
	# A 32x32 px XPM file in /usr/share/pixmaps/ (using 1024x1024 px PNG instead, like VS Code uses for its icon)
	[void](mkdir "${generalOutFolder}/deb/usr/share/pixmaps")
	Copy-Item -Path "./src/Pororoca.Desktop.Debian/pororoca_icon_1024px.png" -Destination "${generalOutFolder}/deb/usr/share/pixmaps/pororoca.png"
	# Hicolor icons
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons")
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor")
	# A 16x16 px PNG file in /usr/share/icons/hicolor/16x16/apps/
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor/16x16")
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor/16x16/apps")
	Copy-Item -Path "./src/Pororoca.Desktop.Debian/pororoca_icon_16px.png" -Destination "${generalOutFolder}/deb/usr/share/icons/hicolor/16x16/apps/pororoca.png"
	# A 32x32 px PNG file in /usr/share/icons/hicolor/32x32/apps/
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor/32x32")
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor/32x32/apps")
	Copy-Item -Path "./src/Pororoca.Desktop.Debian/pororoca_icon_32px.png" -Destination "${generalOutFolder}/deb/usr/share/icons/hicolor/32x32/apps/pororoca.png"
	# A 48x48 px PNG file in /usr/share/icons/hicolor/48x48/apps/
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor/48x48")
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor/48x48/apps")
	Copy-Item -Path "./src/Pororoca.Desktop.Debian/pororoca_icon_48px.png" -Destination "${generalOutFolder}/deb/usr/share/icons/hicolor/48x48/apps/pororoca.png"
	# A 64x64 px PNG file in /usr/share/icons/hicolor/64x64/apps/
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor/64x64")
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor/64x64/apps")
	Copy-Item -Path "./src/Pororoca.Desktop.Debian/pororoca_icon_64px.png" -Destination "${generalOutFolder}/deb/usr/share/icons/hicolor/64x64/apps/pororoca.png"
	# A 128x128 px PNG file in /usr/share/icons/hicolor/128x128/apps/
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor/128x128")
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor/128x128/apps")
	Copy-Item -Path "./src/Pororoca.Desktop.Debian/pororoca_icon_128px.png" -Destination "${generalOutFolder}/deb/usr/share/icons/hicolor/128x128/apps/pororoca.png"
	# A 256x256 px PNG file in /usr/share/icons/hicolor/256x256/apps/
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor/256x256")
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor/256x256/apps")
	Copy-Item -Path "./src/Pororoca.Desktop.Debian/pororoca_icon_256px.png" -Destination "${generalOutFolder}/deb/usr/share/icons/hicolor/256x256/apps/pororoca.png"
	# A 512x512 px PNG file in /usr/share/icons/hicolor/512x512/apps/
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor/512x512")
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor/512x512/apps")
	Copy-Item -Path "./src/Pororoca.Desktop.Debian/pororoca_icon_512px.png" -Destination "${generalOutFolder}/deb/usr/share/icons/hicolor/512x512/apps/pororoca.png"
	# Optionally, an SVG file in /usr/share/icons/hicolor/scalable/apps/
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor/scalable")
	[void](mkdir "${generalOutFolder}/deb/usr/share/icons/hicolor/scalable/apps")
	Copy-Item -Path "./misc/pororoca_logo.svg" -Destination "${generalOutFolder}/deb/usr/share/icons/hicolor/scalable/apps/pororoca.svg"
	
	# Make .deb file
	dpkg-deb --root-owner-group --build "./out/deb/" "./out/Pororoca_${versionName}_amd64.deb"

	# To run Pororoca from the Terminal, on Debian-installed version:
	# LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/usr/lib/pororoca pororoca	

	# To install Pororoca .deb package:
	# sudo apt install ./out/pororoca_version_amd64.deb

	# To uninstall Pororoca:
	# sudo apt remove pororoca

	Remove-Item $installerFilesFolder -Force -Recurse -ErrorAction Ignore
	Remove-Item "./out/deb" -Force -Recurse -ErrorAction Ignore
}

########################## Execute #############################

Run-Pipeline