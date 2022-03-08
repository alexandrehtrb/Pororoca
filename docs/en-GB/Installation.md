# Installation

## Windows

[Download](https://github.com/alexandrehtrb/Pororoca/releases) the .zip file for your Windows system (`win` or `win7`), then extract this file to a folder, and run the Pororoca.exe file. If you wish, right-click on the file, "Send to", "Desktop (create shortcut)".

*If you are using Windows 7*, download the release that has "win7" in the name, instead of the generic "win" release.

## Mac OSX

[Download](https://github.com/alexandrehtrb/Pororoca/releases) the .zip file for your Mac OS system (`osx`), then extract this file to a folder.

After that, move the Pororoca application to your Applications folder. *Without this, your collections and configurations will not remain saved in your computer.*

It is also necessary that your Mac OS authorizes programs from unidentified developers. There are tutorials on how to authorize in the following links: [link1](https://www.macworld.co.uk/how-to/mac-app-unidentified-developer-3669596/) and [link2](https://support.apple.com/en-sa/guide/mac-help/mh40616/mac).

*If you are using a Mac OS with an Apple Silicon M1 chip and the `osx-arm64` app is not running*, try the `osx-x64` package instead.

## Linux

[Download](https://github.com/alexandrehtrb/Pororoca/releases) the .zip file for your Linux system (`linux`), then extract this file to a folder. After that, you can execute the Pororoca program by clicking on it twice or opening it from your Terminal.

If you want to make HTTP/3 requests, the [libmsquic](https://github.com/microsoft/msquic) package must be installed on your machine. There are installation instructions [here](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/http3?view=aspnetcore-6.0#linux) that apply for the most common Linux distros.

In case your Linux distro does not have the libmsquic package available, you can also build and install it. You will need the [lttng-tools](https://github.com/giraldeau/lttng-tools) package installed and there is a tutorial [here](https://github.com/microsoft/msquic/discussions/2318#discussioncomment-2015375).