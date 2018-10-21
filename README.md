# SGL
A Simple Graphics Library for creating simple and fun little applications by using mathematical primitives.

See the [official documentation](https://veselink1.github.io/SGL-Docs/annotated.html) for more information.

## Instructions for use
These instructions assume you are familiar with and using Visual Studio. 

1. [Download](https://github.com/veselink1/SGL/releases) or build the library (.dll file) and name it SGL.dll.

2. Create a new project and right-click on it in the Solution Explorer and select Add -> Reference... -> Browse... and locate the file.

3. Then right-click on it again and select Add -> Reference... -> Assemblies and make sure that the boxes next to PresentationCore, PresentationFramework and WindowsBase are checked. 

4. Use one of the examples below to test the installation.

## Implementation

SGL is implemented as a thin layer on top of WPF. The API surface is purposefully kept to a minimum. All public methods are blocking so that users don't need to think about the actual multithreaded implementation. The main type users interact with is the Window class. It is implemented as a separate graphical window which allows the user to draw different shapes, graph functions, plot arrays of points etc. The library also exports a couple of primitive shapes, implemented as value types, namely: Point, Ellipse, Rectangle, Line and Label. User input is supported through the instance methods GetInt, GetDouble, GetString etc. on the Window class. Other types of input (selecting a point on the screen, selecting an item from an array etc.) are also supported through a nice graphical interface. 

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
    var w = new Window(+5.0);
    w.StrokeColor = Colors.Indigo;
    
    w.Draw(x => Math.Sin(x));
    w.Draw(new Label(Math.PI / 2, 1, "pi/2"));
    
    w.WaitForExit();
  }
}

```
### See more examples [here](https://github.com/veselink1/SGL/tree/master/examples).

### TODO: Add more examples
