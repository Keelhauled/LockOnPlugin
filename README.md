# LockOnPlugin for Honey Select

## Description
This plugin allows you to keep the camera target on specific parts of the target character.  
The purpose of this is to allow you to really focus on the fun parts without having to manage the camera.

[Video of the mod in action](https://my.mixtape.moe/rgkydu.m4v)

## Installation
1. Install Illusion Plugin Architecture (IPA)
2. Throw the files into HoneySelect\Plugins folder
3. Default hotkey is M, unlock by holding the same button for a bit

## Everything else
- press M to lock the camera target to specific parts of the target character
- press M while locked on to switch between different parts
- hold M while locked on to unlock
- press N to keep the camera angle the same relative to the target character  
(this means you can, for example, keep looking at their face even if they are spinning)
- option to change the list of parts you can switch between
- if you don't want to hide the camera target indicator change HideCameraTarget to False
- lots of other settings including keybindings in the modprefs.ini file
- the rotation locking feature is still a bit wonky

## modprefs.ini
- LockOnHotkey = a single letter, no modifier keys
- RotationHotkey = a single letter, no modifier keys
- LockedZoomSpeed = any number, negative values invert the zooming direction
- LockedMinDistance = any positive number
- LockedTrackingSpeed = any positive number
- BoneList = a list split by "|"
- HideCameraTarget = true or false
