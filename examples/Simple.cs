using System;
using System.Windows.Media;
using SGL;

namespace Simple1
{
    class Program
    {
        /// <summary>
        /// Draws the curve of y = ax^2 + bx + c;
        /// </summary>
        static void Main(string[] args)
        {
            // Creates a new window that can show values in the range [-10.0; +10.0]
            Window w = new Window(+10.0);

            // Marks Ox-> and Oy-> on the graph.
            w.DrawAxes();
            
            w.StrokeColor = Colors.Purple;
            w.StrokeThickness = 4;

            // Read the coefficients for the equation from the user. 
            double a = w.ReadDouble("a = ?");
            double b = w.ReadDouble("b = ?");
            double c = w.ReadDouble("c = ?");
            // Declate the function of the equation using these coefficients.
            double myFunc(double x) => a*x*x + b*x + c;

            // Set the stroke thickness to 4.
            w.StrokeThickness = 4;
        
            // Draw the new graph of the function.
            w.Draw(myFunc);

            // Wait for the user to close the window.
            w.WaitForExit();
        }
    }
}
