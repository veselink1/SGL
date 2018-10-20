# SGL
A Simple Graphing Library for .NET for beginners, meant for creating simple and fun little applications.

See the [official documentation](https://veselink1.github.io/SGL-Docs/annotated.html) for more information.

## Instructions for use
These instructions assume you are familiar with and using Visual Studio. 

1. [Download](https://github.com/veselink1/SGL/releases) or build the library (.dll file) and name it SGL.dll.

2. Create a new project and right-click on it in the Solution Explorer and select Add -> Reference... -> Browse... and locate the file.

3. Then right-click on it again and select Add -> Reference... -> Assemblies and make sure that the boxes next to PresentationCore, PresentationFramework and WindowsBase are checked. 

4. Use one of the examples below to test the installation.

## Examples

```csharp
// Program.cs

using System;
using System.Windows.Media;
using SGL;

class Program
{
  static void Main(string[] args)
  {
    var g = new Graph(range: 5, title: "My Graph");
    g.StrokeColor = Colors.Indigo;
    
    g.Draw(x => Math.Sin(x));
    g.Draw(new Label(Math.PI / 2, 1, "pi/2"));
    
    g.WaitForExit();
  }
}

```

### TODO
