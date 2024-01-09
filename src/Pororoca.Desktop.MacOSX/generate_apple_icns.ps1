# https://stackoverflow.com/questions/12306223/how-to-manually-create-icns-files-using-iconutil
# the icon_1024px.png must be an 832x832 icon centered in a 1024x1024 transparent background
# this script must run on Mac OSX
<#
# How to generate .icns on Linux

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
#>

mkdir MyIcon.iconset
sips -z 16 16     icon_1024px.png   --out MyIcon.iconset/icon_16x16.png
sips -z 32 32     icon_1024px.png   --out MyIcon.iconset/icon_16x16@2x.png
sips -z 32 32     icon_1024px.png   --out MyIcon.iconset/icon_32x32.png
sips -z 64 64     icon_1024px.png   --out MyIcon.iconset/icon_32x32@2x.png
sips -z 128 128   icon_1024px.png   --out MyIcon.iconset/icon_128x128.png
sips -z 256 256   icon_1024px.png   --out MyIcon.iconset/icon_128x128@2x.png
sips -z 256 256   icon_1024px.png   --out MyIcon.iconset/icon_256x256.png
sips -z 512 512   icon_1024px.png   --out MyIcon.iconset/icon_256x256@2x.png
sips -z 512 512   icon_1024px.png   --out MyIcon.iconset/icon_512x512.png
cp icon_1024px.png MyIcon.iconset/icon_512x512@2x.png
iconutil -c icns MyIcon.iconset
rm -R MyIcon.iconset