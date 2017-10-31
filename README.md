# LockOnPlugin for Honey Select

[![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=keelhauledhs%40gmail%2ecom&lc=FI&item_name=Keelhauled&item_number=LockOnPlugin&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donate_LG%2egif%3aNonHosted)

## Description
This plugin allows you to keep the camera target on specific parts of the target character.  
The purpose of this is to allow you to really focus on the fun parts without having to manage the camera.  

[Video of the mod in action](https://my.mixtape.moe/rgkydu.m4v) (old video without many of the new features)

## Installation
1. Install Illusion Plugin Architecture (IPA)
2. Throw the files into "HoneySelect\Plugins" folder
3. Default hotkey is N, unlock by holding the same button for a bit

## Download
The latest release should always be [here](../../releases)

## Settings
Settings are located in "HoneySelect\UserData\modprefs.ini"

### LockOnPlugin.Hotkeys
All hotkeys can be set to false to disable them or set to any value from [keynames.txt](keynames.txt), no modifiers
- LockOnHotkey = lock on to targets in quicktargets.txt on the selected character (default key = N)
- LockOnGuiHotkey = enable/disable displaying clickable targets on the selected character (default key = K)
- PrevCharaHotkey = select previous character in the work menu (default key = false)
- NextCharaHotkey = select next character in the work menu (default key = L)
- ~~RotationHotkey = forces the camera to mimic the targeted parts rotation (default key = false)~~

### LockOnPlugin.Misc
- LockedMinDistance = minimum distance from the target (any positive number)
- LockedTrackingSpeed = how fast the camera follows the target (values between 0.01 and 1.0)
- ShowInfoMsg = whether to display messages about what the mod is doing (true/false)
- ManageCursorVisibility = whether to hide the cursor when either mouse button is down (true/false)
- HideCameraTarget = whether to hide the white camera target indicator thing (true/false)
- ScrollThroughMalesToo = whether to scroll through males too with PrevCharaHotkey/NextCharaHotkey (true/false)
- NearClipPlane = governs how close you can be to objects before clipping through them (values smaller than 0.03 may cause glitches to far away objects)

### LockOnPlugin.Gamepad
- ControllerEnabled = enable or disable gamepad controls completely (true/false)
- ControllerMoveSpeed = right stick camera movement sensitivity (values between 0.0 and 1.0)
- ControllerZoomSpeed = camera zoom sensitivity (values between 0.0 and 1.0)
- ControllerRotSpeed = left stick camera rotation sensitivity (values between 0.0 and 1.0)
- ControllerInvertX = invert look direction on x axis (true/false)
- ControllerInvertY = invert look direction on y axis (true/false)
- ControllerSwapSticks = swap functionality of controller sticks (true/false)
- ControllerMovementNeo = enable or disable movement controls in neo (true/false)

 ### Target settings
 Target settings are located in "\HoneySelect\Plugins\TargetSettings\"
 - quicktargetsfemale/male.txt contains targets for LockOnHotkey
 - normaltargets.txt contains targets for LockOnGuiHotkey
 - customtargets.txt contains additional custom targets that are between two normal targets for LockOnGuiHotkey
 - centertargetweights.txt contains points and weights the CenterTargets position is based on

## Control tips
To adjust fov or camera tilt hold left shift/ctrl and drag with right mouse button while locked on.

The hotkeys are intended to be used with the extra mouse buttons for maximum one handed action.  
A little autohotkey script like this can make this mod a lot more enjoyable to use.
```
#IfWinActive StudioNEO ahk_class UnityWndClass
  XButton2::n ; mouse extra button to lock on
  MButton::k ; middle click to show gui targets
#IfWinActive
```
