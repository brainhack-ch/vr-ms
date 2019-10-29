# vr-ms
OG 2019 - Project 01: Virtual reality to improve brain lesions drawing

## Getting Started

This project was developed under Unity 3D version 2018.3.12f1.

### Prerequisites

DICOM data are not provided. Use your own.
Put your DICOM data in *Assets/_Ressources/*
Set-up the configuration file (*Assets/_Ressources/config.txt*) to load your data. This should be change before starting the application in Unity editor.
The configuration file format is :
 - 1st line -> subfolder name of Assets/_Ressources/where to find DICOM data
 - 2nd line -> DICOM_WINDOW_WIDTH
 - 3rd line -> DICOM_WINDOW_CENTER
 
You can then start the application in Unity editor (some path should be changed in *_DicomManager.cs* script for a builded version)

### Commands

To have acces to commands, first select *Mode/Markers* with your right controller, on the left controller interface that appears when looking at your left hand when orthogonal to your view. If not understandable, just move your left controller in all directions until you see the interface. Selection is done by clicking on the desired menu with the ball located at the extremity of the right controller.
Once *Mode/Markers* is selected, commands are :

On Left controller :
- PrimaryTrigger -> switch view between 'only transparent' and 'transparent + slices'
- Button X pressed -> move the slices on all 3 axis
- Orient left controller in a direction + click on Thumbstik -> slice will move by 1 unit in controller direction

On Right controller :
- Button A -> place marker
- Button B -> remove selected marker

Issue : There is actually an offset of 1/2 unit when trying to remove a cube. Just move your pointer around the desired cube to delete it if you don't manage to remove it while pointer is inside.

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](LICENSE) file for details.
Some external assets were used and modified. Original project licences applies on these assets and rights remains to their developers. See section **Aknowledgements** for more informations.

## Acknowledgments
* [EvilDicom](https://github.com/rexcardan/Evil-DICOM) - library to load DICOM
* [Hover-UI-Kit](https://github.com/aestheticinteractive/Hover-UI-Kit) - We modified the Hovercast interface for our interface
* [3D Scifi Kit Starter Kit](https://assetstore.unity.com/packages/3d/environments/3d-scifi-kit-starter-kit-92152) - Free cool sci-fi environment available on Unity asset store

Click on names to have further information about original projects, developpers, and related licences.