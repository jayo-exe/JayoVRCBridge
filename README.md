# Jayo's VRC-OSC Bridge for VNyan

Provides a _complete_ implementation of VRChat's OSC Interface. Respond to avatar parameter changes in the Node Graph and use triggers to drive parameters, inputs, and tracking!

# Table of contents
1. [Installation](#installation)
2. [Usage](#usage)
    1. [Inbound Parameter Changes](#inbound-parameter-changes)
    1. [Outbound Triggers](#outbound-triggers)
        1. [Set an Avatar Parameter](#set-an-avatar-parameter)
        2. [Set an Input Axis](#set-an-input-axis)
        3. [Press or Release an Input Button](#press-or-release-an-input-button)
        4. [Post to the Chatbox](#press-or-release-an-input-button)
        5. [Change Chatbox Typing State](#press-or-release-an-input-button)
        6. [Set Tracker Position or Rotation](#set-tracker-position-or-rotation)
        7. [Eye Tracking Control](#eye-tracking-control)
            1. [Eyes Closed Amount](#eyes-closed-amount)
            2. [Set Eyes Center Pitch Yaw](#set-eyes-center-pitch-yaw)
            3. [Set Eyes Center Pitch Yaw Dist](#set-eyes-center-pitch-yaw-dist)
            4. [Set Eyes Center Vector](#set-eyes-center-vector)
            5. [Set Eyes Center Vector Full](#set-eyes-center-vector-full)
            6. [Set Eyes Left Right Pitch Yaw](#set-eyes-left-right-pitch-yaw)
            7. [Set Eyes Left Right Vector](#set-eyes-left-right-vector)
3. [Development](#development)

## Installation
I've made the source of this plugin available here on Github for anyone to build, run, or modify for their own purposes! 

Built and ready-to-use copies are available for purchase on my [itch.io](https://jayo-exe.itch.io/vrchat-osc-bridge-for-vnyan) ! You'll get ready-to-use plugin files, access to plugin updates forever, and my gratitude for supporting my continued work!

Once you've got your plugin files (either from a DLL file from the store, or from your builds from source), installation is simple:

1. In VNyan, make sure you've enabled "Allow 3rd party plugins" from the Settings menu.
2. Copy the DLL file _directly_ into your VNyan installation folder's `Items\Assemblies` folder
3. Launch VNyan, confirm that a button for the plugin now exists in your Plugins window!

## Usage

This plugin contains an OSC Sender and an OSC Receiver that can connect to VRChat and communicate between VRC and VNyan.  
This is a complete implementation of the VRChat OSC interface documented here: https://docs.vrchat.com/docs/osc-overview .  
Changes to avatar parameters in VRC will be set as VNyan parameters, and will activate a trigger so that your node graph can respond in real time.  
VNyan trigger can also be used to change VRC avatar parameters, drive control inputs, populate the chatbox, and send body-tracking and eye-tracking telemetry!

In order to avoid conflict with other plugins or VNyan internals, parameter and trigger names related to this plugin are prefixed with `_xjvb_`.

### Inbound Parameter Changes

Whenever an Avatar Parameter changes on your VRChat avatar, two things will happen in VNyan immediately:

- **A Float Parameter is set with the received value**.  the name of this parameter is prefixed as described above, so if a parameter called `parameterName` is changed on the avatar, VNyan will update the value of the `_xjvb_parameterName` Float Parameter.
- **A VNyan trigger is fired with the parameter details on its value sockets**. This trigger is named `_xjvb_paramChange`, the name of the VRC Avatar Parameter will be on the `text1` value socket, and a string-encoded float representing the new value will be on the `text2` value socket. 

Use this to sync up elements of your VNyan avatar and world with your VRChat avatar's state, activate haptics, or anything else that can be driven by VNyan's Node Graph!

In addition, whenever a new local VRChat avatar is loaded, a VNyan trigger is fired.  This one is named `_xjvb_avatarChange`, and the ID of the loaded avatar will be on the `text1` value socket.  We'll also set a VNyan String Parameter called `_xjvb_avatar` to this ID.

For more technincal info, check out VRChat's documentation of these features: https://docs.vrchat.com/docs/osc-avatar-parameters

### Outbound Triggers

You can call specially-named tiggers in the VNyan Node Graph to communicate back to VRChat! A few different tpyes of messages are supported, allowing control over avatar parameters, nearly-complete control over avatar movement/inputs, as well as tracking information.

#### Set an Avatar Parameter

Use this trigger to set an Avatar Parameter on your VRChat Avatar from within VNyan.  This could be used to allow VNyan redeems that work inside VRChat if you've built the necessary animations into your avatar!

Trigger Name: `_xjvb_setParam`  
Text1 Value Socket: The name of the VRChat Avatar Parameter to set  
Text2 Value Socket: string representation of the value to be set

If successful, this parameter change will also result in an inbound parameter change behaviour as described in the **Inbound Parameter Changes** section above.

_Relevant VRChat Documentation:_  https://docs.vrchat.com/docs/osc-avatar-parameters

#### Set an Input Axis

Use this trigger to set the value of a VRChat Input Axis from within VNyan.  This could be used to allow external control of your avatar from VNyan or any service connected to the Node Graph.
Per VRChat's documentation, an Input Axis should be set to a float value between -1.0 and 1.0, where 0 means the axis is "neutral" or not in use.  Valid axis names are:
- `Vertical`
- `Horizontal`
- `LookHorizontal`
- `UseAxisRight`
- `GrabAxisRight`
- `MoveHoldFB`
- `SpinHoldCwCcW`
- `SpinHoldUD`
- `SpinHoldLR`

Trigger Name: `_xjvb_setAxis`  
Text1 Value Socket: The name of the VRChat INput Axis to set  
Text2 Value Socket: string representation of the value to be set

_Relevant VRChat Documentation:_  https://docs.vrchat.com/docs/osc-as-input-controller#axes

#### Press or Release an Input Button

Use this trigger to "press" or "release" a VRChat Input Button from within VNyan.  This could be used to allow external control of your avatar from VNyan or any service connected to the Node Graph.
Per VRChat's documentation, an Input Button should be set to 1 to indicate when the button has been pressed, and 0 after it has been released. A button needs to be reset to 0 before setting it to 1 again will have any effect.  Valid button names are:
- `MoveForward`
- `MoveBackward`
- `MoveLeft`
- `MoveRight`
- `LookLeft`
- `LookRight`
- `Jump`
- `Run`
- `ComfortLeft`
- `ComfortRight`
- `DropRight`
- `UseRight`
- `GrabRight`
- `DropLeft`
- `UseLeft`
- `GrabLeft`
- `PanicButton`
- `QuickMenuToggleLeft`
- `QuickMenuToggleRight`
- `Voice`

Trigger Names: `_xjvb_buttonOn` and `_xjvb_buttonOff`  
Text1 Value Socket: The name of the VRChat Input Button to press/release

_Relevant VRChat Documentation:_  https://docs.vrchat.com/docs/osc-as-input-controller#buttons

#### Post to the Chatbox

Use this Trigger to post a message to the chatbox.  Per VRChat's documentation, the chatbox can only display 144 characters, and no more than 9 lines.

Trigger Name: `_xjvb_setChatbox`  
value1 Value Socket: 1 if chat message should be send immediately, 0 if it should populate into the virtual keyboard  
value2 Value Socket: 1 if the chatbox notification cound should play with this update, 0 if it should be silent  
text1 Value Socket: the new contents of the chatbox

_Relevant VRChat Documentation:_  https://docs.vrchat.com/docs/osc-as-input-controller#chatbox

#### Change Chatbox Typing State

Use this trigger to toggle the "typing" indicator in the chatbox.

Trigger Name: `_xjvb_setTyping`  
value1 Value Socket: 1 to turn the typing indicator on, 0 to turn it off

_Relevant VRChat Documentation:_  https://docs.vrchat.com/docs/osc-as-input-controller#chatbox

#### Set Tracker Position or Rotation

VRChat supports position and rotation tracking for up to 8 OSC body Trackers, as well as head tracking.  Use these triggers to to update the tracker telemetry to control the avatar's movements and pose.

Note that **there is no built-in mechnaism in VNyan to get this information in a format that is usable here**! Tracker telemetry triggers are provided for feature-completeness with VRChat's OSC spec. 

Trigger Names: `_xjvb_trackerPos` and `_xjvb_trackerRot`  
value1 Value Socket: the ID of the tracker to set (use 0 for the head tracker)  
text1 Value Socket: string representation of the X coordinate of the position or rotation vector  
text2 Value Socket: string representation of the Y coordinate of the position or rotation vector  
text3 Value Socket: string representation of the Z coordinate of the position or rotation vector

_Relevant VRChat Documentation:_  https://docs.vrchat.com/docs/osc-trackers

#### Eye Tracking Control

VRChat supports eye tracking over OSC.  This is handled by sending information about the amount that the eyes are open, and eye gaze information in one of six different formats.
The finer points of how this works is out-of-scope for this document, please refer to VRChat's documentation to understand how it works.

Note that **there is no built-in mechanismin VNyan to get current Gaze information and convert it to any of these formats**! Eye tracking triggers are provided for feature-completeness with VRChat's OSC spec.

_Relevant VRChat Documentation:_  https://docs.vrchat.com/docs/osc-eye-tracking

##### Eyes Closed Amount

Trigger Name: `_xjvb_eyeClosed`  
text1 Value Socket: string representation of a float between 0.0 and 1.0 indicating how closed the eyes are

##### Set Eyes Center Pitch Yaw

Trigger Name: `_xjvb_eyePitchYaw`  
text1 Value Socket: string representation of a float for the pitch of the eye gaze  
text2 Value Socket: string representation of a float for the yaw of the eye gaze

##### Set Eyes Center Pitch Yaw Dist

Trigger Name: `_xjvb_eyePitchYawDist`  
text1 Value Socket: string representation of a float for the pitch of the eye gaze  
text2 Value Socket: string representation of a float for the yaw of the eye gaze  
text3 Value Socket: string representation of a float for the distance of the eye gaze

##### Set Eyes Center Vector

Trigger Name: `_xjvb_eyeCenterVec`  
text1 Value Socket: string representation of the X coordinate of the gaze vector  
text2 Value Socket: string representation of the Y coordinate of the gaze vector  
text3 Value Socket: string representation of the Z coordinate of the gaze vector

##### Set Eyes Center Vector Full

Trigger Name: `_xjvb_eyeCenterVecFull`  
text1 Value Socket: string representation of the X coordinate of the gaze vector  
text2 Value Socket: string representation of the Y coordinate of the gaze vector  
text3 Value Socket: string representation of the Z coordinate of the gaze vector

##### Set Eyes Left Right Pitch Yaw

Trigger Name: `_xjvb_eyeLRPitchYaw`  
text1 Value Socket: string respresentation of two floats (separated by commas) for the pitch and yaw of the left eye, e.g. `1.234,56.7`  
text2 Value Socket: string respresentation of two floats (separated by commas) for the pitch and yaw of the right eye, e.g. `1.234,56.7`

##### Set Eyes Left Right Vector

Trigger Name: `_xjvb_eyeLRVec`  
text1 Value Socket: string respresentation of three floats (separated by commas) for the gaze vector of the left eye,  e.g. `1.234,56.7,-8`  
text2 Value Socket: string respresentation of three floats (separated by commas) for the gaze vector of the right eye, e.g. `1.234,56.7,-8`

## Development
(Almost) Everything you'll need to develop a fork of this plugin (or some other plugin based on this one)!  The main VS project contains all of the code for the plugin DLL, and the `dist` folder contains a `unitypackage` that can be dragged into a project to build and modify the UI and export the modified Custom Object.

Per VNyan's requirements, this plugin is built under **Unity 2020.3.40f1** , so you'll need to develop on this version to maintain compatability with VNyan.
You'll also need the [VNyan SDK](https://suvidriel.itch.io/vnyan) imported into your project for it to function properly.
Your Visual C# project will need to mave the paths to all dependencies updated to match their locations on your machine (i.e. you VNyan installation directory under VNyan_Data/Managed).  Most should point to Unity Engine libraries for the correct Engine version **2020.3.40f1**.
