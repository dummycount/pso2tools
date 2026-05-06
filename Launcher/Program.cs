// This program launches a program of the same name in a subfolder of the same name.
// For example, if it is named App.exe, it will launch App/App.exe.

using System.Diagnostics;

var self = Environment.GetCommandLineArgs()[0];
var name = Path.GetFileName(self);
var target = Path.Join(Path.GetDirectoryName(self), Path.ChangeExtension(name, ""), name);

Process.Start(target, args);
