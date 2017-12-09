# LockOnPlugin for Honey Select

[![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=keelhauledhs%40gmail%2ecom&lc=FI&item_name=Keelhauled&item_number=LockOnPlugin&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donate_LG%2egif%3aNonHosted)

## Description
This plugin allows you to keep the camera target on specific parts of the target character.  
The purpose of this is to allow you to really focus on the fun parts without having to manage the camera.  

[Video of the mod in action](https://gfycat.com/GranularBrilliantBuck)

## Installation
1. Install Illusion Plugin Architecture (IPA)
2. Throw the files into the Honey Select root folder
3. Default hotkey is N, unlock by holding the same button for a bit

## Download
The latest release should always be [here](https://github.com/Keelhauled/LockOnPlugin/releases)

## Settings
Settings are located in ```\UserData\modprefs.ini```

### Hotkeys
All hotkeys can be set to false to disable or set to any value from [keynames.txt](keynames.txt), no modifiers
- ```LockOnHotkey``` Lock to targets in quicktargets.txt on the selected character (default: N)
- ```LockOnGuiHotkey``` Display clickable targets on the selected character (default: K)
- ```PrevCharaHotkey``` Select previous character in the work menu (default: false)
- ```NextCharaHotkey``` Select next character in the work menu (default: L)

### Misc
- ```LockedTrackingSpeed``` How fast the camera follows the target (between 0.01 and 1.0)
- ```ShowInfoMsg``` Display messages about what the mod is doing (true/false)
- ```ManageCursorVisibility``` Hide the cursor when either mouse button is down (true/false)
- ```HideCameraTarget``` Hide the white camera target indicator thing (true/false)
- ```ScrollThroughMalesToo``` Scroll through males too with NextCharaHotkey (true/false)
- ```NearClipPlane``` Governs how close you can be to objects before clipping through <br> (values smaller than 0.03 may cause glitches to distant objects)

### Gamepad
- ```ControllerEnabled``` Enable or disable gamepad controls completely (true/false)
- ```ControllerMoveSpeed``` Camera movement sensitivity (between 0.0 and 1.0)
- ```ControllerZoomSpeed``` Camera zoom sensitivity (between 0.0 and 1.0)
- ```ControllerRotSpeed``` Camera rotation sensitivity (between 0.0 and 1.0)
- ```ControllerInvertX``` Invert look direction on x axis (true/false)
- ```ControllerInvertY``` Invert look direction on y axis (true/false)
- ```ControllerSwapSticks``` Swap functionality of controller sticks (true/false)

### Target settings
Target settings are located in ```\Plugins\TargetSettings\```
- ```quicktargets(fe)male.txt``` contains targets for LockOnHotkey
- ```normaltargets.txt``` contains targets for LockOnGuiHotkey
- ```customtargets.txt``` contains additional targets that are between two normal targets
- ```centertargetweights.txt``` contains points and weights the CenterTarget position is based on

## Control tips
To adjust fov or camera angle hold left shift/ctrl and drag with right mouse button while locked on.

Gamepad input requires a xinput compatible controller. (Movement only works in neo)  
Use DS4Windows or something similar if you have a directinput controller.

The hotkeys should be used with the extra mouse buttons for maximum one handed action.  
A little autohotkey script like this can make this mod a lot more enjoyable to use.
```
#IfWinActive StudioNEO ahk_class UnityWndClass
  XButton2::n ; mouse extra button to lock on
  MButton::k ; middle click to show gui targets
#IfWinActive
```
