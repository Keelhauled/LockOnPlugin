# LockOnPlugin for Honey Select

## Description
This plugin allows you to keep the camera target on specific parts of the target character.  
The purpose of this is to allow you to really focus on the fun parts without having to manage the camera.

[Video of the mod in action](https://my.mixtape.moe/rgkydu.m4v)

## Installation
1. Install Illusion Plugin Architecture (IPA)
2. Throw the files into "HoneySelect\Plugins" folder
3. Default hotkey is N, unlock by holding the same button for a bit

## Download
The latest release should always be [here](../../releases)

## Settings
Settings are located in "HoneySelect\UserData\modprefs.ini"

### Hotkeys
All hotkeys can be set to false to disable them or set to any value from [keynames.txt](keynames.txt), no modifiers
- LockOnHotkey = lock on to targets in quicktargets.txt on the selected character (default key = N)
- LockOnGuiHotkey = enable/disable displaying clickable targets on the selected character (default key = K)
- PrevCharaHotkey = select previous character in the work menu (default key = false)
- NextCharaHotkey = select next character in the work menu (default key = L)

### Other settings
- LockedZoomSpeed = how fast zooming is when locked (negative values invert the zooming direction)
- LockedMinDistance = minimum distance from the target (any positive number)
- LockedTrackingSpeed = how fast the camera follows the target (any positive number)
- ShowInfoMsg = whether to display messages about what the mod is doing (true/false)
- ManageCursorVisibility = whether to hide the cursor when either mouse button is down (true/false)
- HideCameraTarget = whether to hide the white camera target indicator thing (true/false)
- ScrollThroughMalesToo = whether to scroll through males too with PrevCharaHotkey/NextCharaHotkey (true/false)

 ### Target settings
 Target settings are located in "\HoneySelect\Plugins\LockOnPlugin\"
 - quicktargets.txt contains targets for LockOnHotkey
 - normaltargets.txt contains targets for LockOnGuiHotkey
 - customtargets.txt contains additional custom targets that are between two normal targets for LockOnGuiHotkey

## Control tips
To adjust fov or camera tilt hold left shift/ctrl and drag with right mouse button while locked on.

The hotkeys are intended to be used with the extra mouse buttons for maximum one handed action.  
A little autohotkey script like this can make this mod a lot enjoyable to use.
```
#IfWinActive StudioNEO ahk_class UnityWndClass
  XButton2::n ; mouse extra button to lock on
  MButton::k ; middle click to show gui targets
#IfWinActive
```
