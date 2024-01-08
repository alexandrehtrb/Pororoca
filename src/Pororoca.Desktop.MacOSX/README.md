# How to generate .icns icons for Mac OSX

https://dentrassi.de/2014/02/25/creating-mac-os-x-icons-icns-on-linux/

```
-rwxr-xr-x 0 jens jens   1427 Feb 24 10:49 icon_16px.png
-rwxr-xr-x 0 jens jens   2003 Feb 24 10:49 icon_32px.png
-rwxr-xr-x 0 jens jens   2560 Feb 24 10:48 icon_48px.png
-rwxr-xr-x 0 jens jens   5304 Feb 24 10:48 icon_128px.png
-rwxr-xr-x 0 jens jens   9883 Feb 24 10:47 icon_256px.png
-rwxr-xr-x 0 jens jens   9883 Feb 24 10:47 icon_512px.png
-rwxr-xr-x 0 jens jens   9883 Feb 24 10:47 icon_1024px.png
```

```bash
sudo apt-get install icnsutils
```

```bash
png2icns icon.icns icon_*px.png
```