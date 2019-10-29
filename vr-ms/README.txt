# vr-ms
OG 2019 - Project 01: Virtual reality to improve brain lesions drawing

Unity version : 2018.3.12f1
scene in Assets/scene/_main_scene

Config file to load data (change it before playing in editor)
location : Assets/_Ressources/config.txt
format :
1st line -> subfolder name of Assets/_Ressources/where to find DICOM data
2nd line -> DICOM_WINDOW_WIDTH
3rd line -> DICOM_WINDOW_CENTER

Currently set-up to work in editor (some path should change in _DicomManager script for build version)

Controls :

- if mode/markers selected on left controller :
Left controller :
PrimaryTrigger -> switch view between 'only transparent' and 'transparent + slices'
Button X pressed -> move the slices on all 3 axis
orient left controller in a direction + click on Thumbstik -> slice will move by 1 unit in controller direction
Right controller :
Button A -> place marker
Button B -> remove selected marker

Issues : there is actually some offset of 1/2 unit when trying to remove a cube



Use a modified version of the Hovercast interface of the Hover-UI-Kit (https://github.com/aestheticinteractive/Hover-UI-Kit)
Use 3D Scifi Kit Starter Kit assets for free cool sci-fi environment(https://assetstore.unity.com/packages/3d/environments/3d-scifi-kit-starter-kit-92152)
Use EvilDicom library to load DICOM data (https://github.com/rexcardan/Evil-DICOM)
