# PSO2 Tools

Programs for working with PSO2 file formats. See [the releases page](https://github.com/dummycount/pso2tools/releases) for downloads.

## Prerequisites

The following must be installed for these tools to run:

- [Windows App SDK 1.8.6 (x64)](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads)
- [.NET Desktop Runtime 10 (x64)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Visual C++ v14 Redistributable](https://aka.ms/vc14/vc_redist.x64.exe)

## CMX Viewer

Run CmxViewer.exe to start the program.

This is a program for viewing the "character making index", the list of character creation parts available in the game. It reads data from your local installation of the game. If it fails to find the game's location automatically, go to settings and set the "path to pso2_bin folder" setting.

Select an item type on the left and it will display a list of all item of that type. Select an item to view its data and the game files related to it.

The Color Sets page lists the default colors associated with layering wear. When an item has multiple color variants (e.g. /2, /3, etc. versions), these generally do not have their own entries in the index. The color information is stored here instead.
