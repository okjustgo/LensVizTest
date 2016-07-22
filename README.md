# LensVizTest


This should get you going
https://developer.microsoft.com/en-us/windows/holographic/install_the_tools

When you open the project in Unity it will complain that it's an earlier version, that is ok. 
Once you open the project you need to then open MainScene.unity. 

You'll also want this to edit your CS files in VS: https://visualstudiogallery.msdn.microsoft.com/8d26236e-4a64-4d64-8486-7df95156aba9

Here are the steps for building a UWP app for HoloLens using Unity: https://developer.microsoft.com/en-us/windows/holographic/holograms_100

If you want to debug the Azure blob storage REST requests in Unity using Fiddler, you'll need to do some configuration to both: https://blogs.msdn.microsoft.com/wsdevsol/2015/02/16/setting-up-fiddler-to-capture-network-traffic-from-the-unity-3d-editor/

When connecting to Azure in a UWP, you'll need to enable the following settings in Package.appxmanifest:
	- Internet (Client & Server)
	- Internet (Client)
	- Private Networks (Client & Server)