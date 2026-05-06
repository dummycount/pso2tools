
$Framework = 'net10.0-windows10.0.26100.0'
$ReleasePath = "$PSScriptRoot/release"

$Programs = @{
    'CmxViewer' = "CmxViewer/bin/x64/Release/$Framework/win-x64"
    'Defrost'   = "Defrost/bin/x64/Release/$Framework/win-x64"
}

Remove-Item $ReleasePath -Recurse
New-Item -ItemType Directory $ReleasePath

# Finding the .exe file in the release folder is a pain, and there's no good way to
# create a shortcut file without an intaller. Instead, build a launcher program that
# runs the actual program of the same name.
dotnet publish Launcher -r win-x64 -c Release /p:PublishSingleFile=true
$LauncherPath = "$PSScriptRoot/Launcher/bin/Release/net10.0/win-x64/publish/Launcher.exe"

foreach ($Name in $Programs.Keys) {
    $Source = "$PSScriptRoot/$($Programs[$Name])"
    $Dest = "$ReleasePath/$Name"

    # Copy the release build
    Copy-Item -Path $Source -Destination $Dest -Recurse

    # Make a copy of the launcher in the root directory as a shortcut
    Copy-Item -Path $LauncherPath -Destination "$ReleasePath/$Name.exe"
}

Compress-Archive -Path "$ReleasePath/*" -DestinationPath "$ReleasePath/Pso2Tools.zip" -CompressionLevel Optimal
