# PSO2 Tools

Windows programs for working with PSO2 file formats. See [the releases page](https://github.com/dummycount/pso2tools/releases) for downloads.

Each program is in its own subfolder. The .exe files in the root folder of the release .zip file are simply links to the actual programs for convenience.

## Prerequisites

The following must be installed for these tools to run:

- [Windows App SDK 2.0.1 (x64)](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads)
- [.NET Desktop Runtime 10 (x64)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Visual C++ v14 Redistributable](https://aka.ms/vc14/vc_redist.x64.exe)

## CMX Viewer

Run CmxViewer.exe to start the program.

This is a program for viewing the "character making index", the list of character creation parts available in the game. It reads data from your local installation of the game. If it fails to find the game's location automatically, go to settings and set the "path to pso2_bin folder" setting.

Select an item type on the left and it will display a list of all item of that type. Select an item to view its data and the game files related to it.

The Color Sets page lists the default colors associated with layering wear. When an item has multiple color variants (e.g. /2, /3, etc. versions), these generally do not have their own entries in the index. The color information is stored here instead.

## Defrost

Run Defrost.exe to start the program.

This is a program for inspecting and extracting ICE format archives. Click the "open" button and select a file, or drag and drop a file onto the program to open it.

### Extracting files

To extract all files in the archive, click "extract all". A window will open where you can select the destination and other options for extracting.

To extract individual files, drag and drop them into File Explorer. You can also select the files to extract, click copy (or press <kbd>Ctrl+C</kbd>) and paste them where you want them.

Double clicking a file (or pressing <kbd>Enter</kbd> with files selected) will extract it to a temporary file and open it in your default program for that file type.

### Open from CMX Viewer

CMX viewer has an option to open a file in Defrost, but Defrost first needs to be registered as a file handler. To do this, open Defrost, go to settings, and scroll down to the "file associations" section. Click the "open Defrost with pso2defrost:// links" button. The text below it should change to "registered".

### File preview

When a single file is selected, a preview of it will appear on the right side. You can disable this with the button in the top right.

Preview currently supports the following file types:

- `.aqp`, `.trp` - 3D models

	- Models with transparency may not render correctly.
	- Models with multiple sets of UVs (like NGS hair models) do not currently render correctly.
	- This will attempt to find and load textures from the archive. This may not work for all models. If you find a model where textures are present but aren't loaded, let me know.
	- If a diffuse texture has a matching color mask texture, it will colorize the texture. This uses a heuristic to determine which color channels are used, so it may not match the behavior in game.
	- If a mesh uses the NGS skin material, it will load skin textures from your local installation of the game. The game install location and which textures to load can be set in settings.
	- If a material looks like it is for a CAST part, textures are adjusted so the model's UVs line up with the textures.

- `.dds` - Textures

	- You can hide the alpha channel or view individual channels with the controls at the top.

- `.lua` - Lua script
- `.text` - Text data

### Planned features

The following features are not yet implemented. I'll maybe get to them eventually:

- Edit and save ICE archives
- Customize which program opens for each file extension instead of using Windows' file associations
- Preview for more file types:
	- `.aqn`, `.trn` - skeleton
	- `.fltd` - physics
	- `.lac` - lobby action command
	- `.mso` - my space object
	- `.tcb` - terrain
	- `.txl` - texture list
	- `.wdsn` - window design
	- Let me know if there are other file formats you want to see supported
- More right click menu options
	- Export models to FBX?
	- Export textures to PNG?
	- Export text files?

## Building

Install these prerequisites:

- [FBX SDK 2020.1](https://www.autodesk.com/content/dam/autodesk/www/adn/fbx/2020-1/fbx20201_fbxsdk_vs2017_win.exe)
- [Visual Studio 2026](https://visualstudio.microsoft.com/) with the ".NET desktop development" and "WinUI application development" workloads
- [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

Clone this repo with submodules:

```sh
git clone --recurse-submodules https://github.com/dummycount/pso2tools.git
```

Run the setup_fbx_sdk.ps1 script to link the FBX dependencies:

```sh
./setup_fbx_sdk.ps1
```

Open [Pso2Tools.slnx](Pso2Tools.slnx) and build the solution.

### Making a Release

Build the solution in release mode, then run the make_release.ps1 script:

```sh
./make_release.ps1
```

This will copy the programs into a `release` folder and create a .zip archive.
