# PSP Save Manager
This is, as the name would suggest, a save manager for PSP save files. 

It automatically scans connected drives and MTP devices to find the right save directories. 

Right now it allows you to copy, overwrite and delete saves directly between PPSSPP save folders and real PSP memory cards 
connected as Windows removable drives, MTP or over FTP

![Preview image](preview1.png)

Devices that currently support autodetection:
- Real PSP memory card connected directly to PC
- PS Vita (vitashell USB)
- PS Vita Content Manager
- Nintendo Switch over MTP
- Android over MTP

In addition to those you can add your own directories or FTP servers

# How to build
Open the sln in Visual Studio and that should be it, but if it doesn't work download the MediaDevices nuget package