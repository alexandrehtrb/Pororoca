# This spec file is for SUSE (openSUSE, SLES and SLED).

# .NET Linux required packages: https://github.com/dotnet/core/blob/main/release-notes/8.0/os-packages.md
# Specifically for SUSE: https://learn.microsoft.com/en-us/dotnet/core/install/linux-sles?tabs=dotnet8#dependencies
# Avalonia required packages: https://docs.avaloniaui.net/tools/parcel/packaging-for-linux#avalonia-specific-dependencies

# package compression algorithm and level
# https://stackoverflow.com/questions/9292243/rpmbuild-change-compression-format

%define _source_payload w7T16.xzdio
%define _binary_payload w7T16.xzdio

# The line below removes liblttng-ust.so.0()(64bit) requirement from .NET 8.0,
# which cannot be provided for SUSE, but it's also not needed for execution.
# https://github.com/dotnet/runtime/issues/57784#issuecomment-3868191774
%global __requires_exclude liblttng-ust.so.

Name:       Pororoca
Version:    3.10.1
Release:    1%{?dist}
Summary:    This is Pororoca, an HTTP testing tool.
License:    GPLv3+
BuildArch:  x86_64
URL:        https://pororoca.io
Requires: ca-certificates
Requires: glibc
Requires: krb5
Requires: libgcc_s1
Requires: libstdc++6
Requires: libicu
Requires: libopenssl3
Requires: timezone
Requires: libz1
Requires: libICE6
Requires: libSM6
Requires: libfontconfig1
Requires: libX11-6

%description
This is Pororoca, an HTTP testing tool.

%prep
# No prep needed - files are already staged

%build
# No build needed - binaries are pre-built

%install
mkdir -p %{buildroot}/usr/bin
cp ~/rpmstaging/others/pororoca.sh %{buildroot}/usr/bin/pororoca
chmod 755 %{buildroot}/usr/bin/pororoca
mkdir -p %{buildroot}/usr/lib/pororoca
install -m 666 -D ~/rpmstaging/binaries/* -t %{buildroot}/usr/lib/pororoca/
chmod +x %{buildroot}/usr/lib/pororoca/Pororoca
mkdir -p %{buildroot}/usr/share/applications
install -m 666 ~/rpmstaging/others/Pororoca.desktop %{buildroot}/usr/share/applications/Pororoca.desktop
mkdir -p %{buildroot}/usr/share/pixmaps
install -m 666 ~/rpmstaging/others/pororoca_icon_1024px.png %{buildroot}/usr/share/pixmaps/pororoca.png
mkdir -p %{buildroot}/usr/share/icons/hicolor/scalable/apps
install -m 666 ~/rpmstaging/others/pororoca_logo.svg %{buildroot}/usr/share/icons/hicolor/scalable/apps/pororoca.svg
mkdir -p %{buildroot}/usr/share/icons/hicolor/16x16/apps
install -m 666 ~/rpmstaging/others/pororoca_icon_16px.png %{buildroot}/usr/share/icons/hicolor/16x16/apps/pororoca.png
mkdir -p %{buildroot}/usr/share/icons/hicolor/32x32/apps
install -m 666 ~/rpmstaging/others/pororoca_icon_32px.png %{buildroot}/usr/share/icons/hicolor/32x32/apps/pororoca.png
mkdir -p %{buildroot}/usr/share/icons/hicolor/48x48/apps
install -m 666 ~/rpmstaging/others/pororoca_icon_48px.png %{buildroot}/usr/share/icons/hicolor/48x48/apps/pororoca.png
mkdir -p %{buildroot}/usr/share/icons/hicolor/64x64/apps
install -m 666 ~/rpmstaging/others/pororoca_icon_64px.png %{buildroot}/usr/share/icons/hicolor/64x64/apps/pororoca.png
mkdir -p %{buildroot}/usr/share/icons/hicolor/128x128/apps
install -m 666 ~/rpmstaging/others/pororoca_icon_128px.png %{buildroot}/usr/share/icons/hicolor/128x128/apps/pororoca.png
mkdir -p %{buildroot}/usr/share/icons/hicolor/256x256/apps
install -m 666 ~/rpmstaging/others/pororoca_icon_256px.png %{buildroot}/usr/share/icons/hicolor/256x256/apps/pororoca.png
mkdir -p %{buildroot}/usr/share/icons/hicolor/512x512/apps
install -m 666 ~/rpmstaging/others/pororoca_icon_512px.png %{buildroot}/usr/share/icons/hicolor/512x512/apps/pororoca.png

%files
/usr/bin/pororoca
/usr/lib/pororoca/*
/usr/share/applications/Pororoca.desktop
/usr/share/pixmaps/pororoca.png
/usr/share/icons/hicolor/scalable/apps/pororoca.svg
/usr/share/icons/hicolor/16x16/apps/pororoca.png
/usr/share/icons/hicolor/32x32/apps/pororoca.png
/usr/share/icons/hicolor/48x48/apps/pororoca.png
/usr/share/icons/hicolor/64x64/apps/pororoca.png
/usr/share/icons/hicolor/128x128/apps/pororoca.png
/usr/share/icons/hicolor/256x256/apps/pororoca.png
/usr/share/icons/hicolor/512x512/apps/pororoca.png

%changelog
# Let's skip this for now
