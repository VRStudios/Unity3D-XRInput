# Simple XRInput for Unity3D & whats it for
This API acts as its own easy-to-use agnostic XR-Input layer allowing you to write the bulk of your code against it then easily target multiple platforms (namely Steam, Oculus, WMR, etc) without having to deal with the endless flux of complication or fragmentation between different companies or even Unity's native input API flux. More non-standard Input sources could be added to if needed (such as Pico) and OpenXR support later when thats stabilized making it so you don't need to modify the Input system in your projects down the road.

# How to setup
* <b>NOTE: SteamVR SDK must NOT be installed for OpenVR Input mode to work!</b>
* Ensure 'com.valvesoftware.unity.openvr' .tgz file is installed via Unity package manager for OpenVR input mode
* Run file-menu 'Assets/VRstudios/XRInput/Enable for Steam' for Steam / OpenVR input mode
* Run file-menu 'Assets/VRstudios/XRInput/Enable for Generic-Unity-Input' for Oculus PC/Android, Generic, etc mode
* Drop 'Unity3D-XRInput/Assets/VRstudios/XRInput' prefab into scene & thats it.

# How to get Input
* Take a look at 'Unity3D-XRInput/Assets/VRstudios/TestInput.cs' for basic working example
* You can get Input in multiple ways
    * Get full input state of controller: XRInput.ControllerState(...)
    * Get specific input state of controller: XRInput.ButtonTrigger(...), XRInput.Button1(...), etc
    * Get specific input state via callbacks: ButtonTriggerOnEvent, ButtonTriggerDownEvent, Button1DownEvent, etc