using System;
using System.Windows.Media;
using SGL;

namespace Rainbow1
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
            
            // To create the rainbow effect, 
            // we must be constantly updating the content of the window,
            // while it is open.
            while (!w.Closed)
            {
                // First, we erase the previous graph (if any).
                w.Erase(myFunc);
                // Then, we update the color of the stroke to be
                // sourced from Utils.Rainbow, which linearly interoplates
                // between the colors of the rainbow, accoring to some value.
                w.StrokeColor = Utils.Rainbow(w.Time / 10.0);
                // We draw the new graph of the function.
                w.Draw(myFunc);
                // And finally, we wait for the screen to 
                // be updated, since if we are constantly changing what 
                // is on screen it would be too resource-intensive.
                w.WaitForUpdate();
            }
        }
    }
}
