using System;
using System.Windows.Media;
using SGL;

namespace Drawing
{
    static class Program
    {
        /// <summary>
        /// Allows the user to draw lines.
        /// </summary>
        static void Main(string[] args)
        {
            // Creates a new window.
            Window w = new Window(+5.0);

            w.StrokeColor = Colors.Purple;
            w.StrokeThickness = 5;

            while (true)
            {
                Color[] availableColors = new Color[]
                {
                    Colors.Indigo,
                    Colors.LightGoldenrodYellow,
                    Colors.DarkOliveGreen,
                    Colors.LightSalmon,
                    Colors.HotPink,
                    Colors.CornflowerBlue,
                };

                // Allows the user to choose an item from the list
                // and returns the selected item.
                Color color = w.SelectItem("Choose a color to draw with.", availableColors);
                w.StrokeColor = color;

                int n = w.ReadInt("Number of points?", 3);

                Point prevPt = w.SelectPoint();
                for (int i = 0; i < n - 1; i++)
                {
                    Point pt = w.SelectPoint();
                    Line a = new Line(pt, prevPt);
                    Label label = new Label(pt, pt.ToString());
                    w.Draw(a);
                    prevPt = pt;
                }
                if (!w.YesOrNo("Do you want to continue drawing?"))
                {
                    break;
                }
            }

            w.Notify("You've done an excellent job!");
            w.WaitForExit();
        }
    }
}
