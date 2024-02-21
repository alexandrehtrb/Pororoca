#!/bin/bash
# this script should be executed in root solution folder
rm -rf ./out/
rm -rf ./staging_folder/
# .NET publish
# self-contained is recommended, so final users won't need to install .NET
dotnet publish "./src/Pororoca.Desktop/Pororoca.Desktop.csproj" \
  --verbosity quiet \
  --nologo \
  --configuration Release \
  -p:PublishForInstallOnDebian=true \
  --self-contained true \
  --runtime linux-x64 \
  --output "./out/linux-x64"
# Staging directory
mkdir staging_folder
# Debian control file
mkdir ./staging_folder/DEBIAN
cp ./src/Pororoca.Desktop.Debian/control ./staging_folder/DEBIAN
# Executable file and script
mkdir ./staging_folder/usr
mkdir ./staging_folder/usr/bin
cp ./src/Pororoca.Desktop.Debian/pororoca.sh ./staging_folder/usr/bin/pororoca
chmod +x ./staging_folder/usr/bin/pororoca # set executable permissions to starter script
# Shared libraries and other files
mkdir ./staging_folder/usr/lib
mkdir ./staging_folder/usr/lib/pororoca
cp -f -a ./out/linux-x64/. ./staging_folder/usr/lib/pororoca/ # copies all files from publish dir
chmod -R a+rX ./staging_folder/usr/lib/pororoca/ # set read permissions to all files
chmod +x ./staging_folder/usr/lib/pororoca/Pororoca # set executable permissions to main executable
# Desktop shortcut
mkdir ./staging_folder/usr/share
mkdir ./staging_folder/usr/share/applications
cp ./src/Pororoca.Desktop.Debian/Pororoca.desktop ./staging_folder/usr/share/applications/Pororoca.desktop
# Desktop icon
# A 1024px x 1024px PNG, like VS Code uses for its icon
mkdir ./staging_folder/usr/share/pixmaps
cp ./src/Pororoca.Desktop.Debian/pororoca_icon_1024px.png ./staging_folder/usr/share/pixmaps/pororoca.png
# Hicolor icons
mkdir ./staging_folder/usr/share/icons
mkdir ./staging_folder/usr/share/icons/hicolor
mkdir ./staging_folder/usr/share/icons/hicolor/scalable
mkdir ./staging_folder/usr/share/icons/hicolor/scalable/apps
cp ./misc/pororoca_logo.svg ./staging_folder/usr/share/icons/hicolor/scalable/apps/pororoca.svg
# Make .deb file
dpkg-deb --root-owner-group --build ./staging_folder/ ./pororoca_3.1.0_amd64.deb