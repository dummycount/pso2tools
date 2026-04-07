#! /usr/bin/env python3

from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).parent

FBX_URL = "https://www.autodesk.com/content/dam/autodesk/www/adn/fbx/2020-1/fbx20201_fbxsdk_vs2017_win.exe"

FBX_SRC = Path("C:/Program Files/Autodesk/FBX/FBX SDK/2020.1")
FBX_DEST = ROOT / "PSO2-Aqua-Library/AquaModelLibrary.Native/Dependencies/FBX"


def check_dependencies():
    if not FBX_SRC.exists():
        print(f"Please install FBX SDK 2020.1: {FBX_URL}")
        sys.exit(1)


def make_junction(src: Path, dest: Path):
    if dest.exists():
        return

    subprocess.call(["mklink", "/J", dest, src], shell=True)


def main():
    check_dependencies()

    # Set up Aqua Library dependencies
    # Use junction points instead of symlinks so Git sees them as directories
    # and they fit PSO2-Aqua-Library's .gitignore patterns.
    make_junction(FBX_SRC / "lib", FBX_DEST / "lib")
    make_junction(FBX_SRC / "include", FBX_DEST / "include")


if __name__ == "__main__":
    main()
