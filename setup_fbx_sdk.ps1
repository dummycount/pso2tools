$FbxUrl = 'https://www.autodesk.com/content/dam/autodesk/www/adn/fbx/2020-1/fbx20201_fbxsdk_vs2017_win.exe'

$FbxSource = 'C:/Program Files/Autodesk/FBX/FBX SDK/2020.1'
$FbxDest = "$PSScriptRoot/PSO2-Aqua-Library/AquaModelLibrary.Native/Dependencies/FBX"

if (-not (Test-Path -Path $FbxSource)) {
    Write-Output "Please install FBX SDK 2020.1: $FbxUrl"
    exit 1
}

function New-Junction([string] $path, [string] $target) {
    if (Test-Path -Path $path) {
        return
    }

    New-Item -ItemType Junction -Path $path -Value $target
}

New-Junction "$FbxDest/lib" "$FbxSource/lib"
New-Junction "$FbxDest/include" "$FbxSource/include"
