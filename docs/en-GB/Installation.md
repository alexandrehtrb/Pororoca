# Installation

The installation packages are available on GitHub [Releases](https://github.com/alexandrehtrb/Pororoca/releases) page. This page is the only official and reliable source for downloading this program.

Currently, there is support only for `x86` and `x64` architectures. The last version that supports `arm` and `arm64` architectures is 1.5.0. You can run x86 and x64 releases on an ARM machine if its operating system supports emulation.

## Windows (`win`)

*Important*: The .exe programs are not signed, therefore, there may be messages from Windows SmartScreen saying that it "prevented an unrecognized app from starting". Just click on "More info" and then on "Run anyway" to continue.

### With installer (`_installer`)

Download the installer for your system and follow the installation steps.

### Portable (`_portable`)

Download and extract the package, then run the `Pororoca.exe` file. If you wish, right-click on the file, "Send to", "Desktop (create shortcut)".

## Mac OSX (`osx`)

Download and extract the package, after that, move the Pororoca application to your Applications folder. *Without this, your collections and configurations will not remain saved in your computer.*

It is also necessary that your Mac OS authorizes programs from unidentified developers. There are tutorials on how to authorize in the following links: [link1](https://www.macworld.co.uk/how-to/mac-app-unidentified-developer-3669596/) and [link2](https://support.apple.com/en-sa/guide/mac-help/mh40616/mac).

*If you are using a Mac OS with an Apple Silicon M1 chip and the `osx-arm64` app is not running*, try the `osx-x64` package instead.

## Linux (`linux`)

*Important*: Pororoca requires msquic version 1.9.0, and using versions higher than that will cause HTTP/3 requests to not run successfully.

Download and extract the package to a folder. After that, you can execute the Pororoca program by clicking on it twice or opening it from your Terminal.

If you want to make HTTP/3 requests, the [msquic](https://github.com/microsoft/msquic) package must be installed on your machine. There are installation instructions [here](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/http3?view=aspnetcore-6.0#linux) that apply for the most common Linux distros.

In case your Linux distro does not have the msquic package available, you can also build and install it. You will need the [lttng-tools](https://github.com/giraldeau/lttng-tools) package installed and there is a tutorial [here](https://github.com/microsoft/msquic/discussions/2318#discussioncomment-2015375). *Be aware on using the correct version of msquic repo:* `git checkout v1.9.0`.