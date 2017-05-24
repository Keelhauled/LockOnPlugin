# LockOnPlugin for Honey Select

## Description
This plugin allows you to keep the camera target on specific parts of the target character.  
The purpose of this is to allow you to really focus on the fun parts without having to manage the camera.

[Video of the mod in action](https://my.mixtape.moe/rgkydu.m4v)

## Installation
1. Install Illusion Plugin Architecture (IPA)
2. Throw the files into HoneySelect\Plugins folder
3. Default hotkey is M, unlock by holding the same button for a bit

## Controls and everything else
- press M to lock the camera target to specific parts of the target character (change parts in quickbones.txt)
- press M while locked on to switch between different parts
- hold M while locked on to unlock
- press K to display lock on buttons for all parts that are specified in the guibones.txt and intersections.txt files
- intersections.txt can be used to create new lock on points directly between two existing parts
- press L to lock on to another character
- press N to keep the camera angle the same relative to the target character
(this means you can, for example, keep looking at their face even if they are spinning)
- the rotation locking feature is still very wonky
- option to change the list of parts you can switch between
- if you don't want to hide the camera target indicator change HideCameraTarget to False
- if you don't want to hide the cursor change ManageCursorVisibility to False
- lots of other settings including keybindings in the modprefs.ini file
- to adjust fov or camera tilt hold left shift/ctrl and drag with right mouse button while locked on

## modprefs.ini
- LockOnHotkey = false to disable or any value from [keynames.txt](keynames.txt), no modifiers
- RotationHotkey = false to disable or any value from [keynames.txt](keynames.txt), no modifiers
- CharaSwitchHotkey = false to disable or any value from [keynames.txt](keynames.txt), no modifiers
- LockOnGuiHotkey = false to disable or any value from [keynames.txt](keynames.txt), no modifiers
- LockedZoomSpeed = any number, negative values invert the zooming direction
- LockedMinDistance = any positive number
- LockedTrackingSpeed = any positive number
- HideCameraTarget = true or false
- ManageCursorVisibility = true or false
