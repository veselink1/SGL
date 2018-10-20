using System;
using System.Windows.Media;

namespace SGL
{
    /// <summary>
    /// A static class containing some utility methods.
    /// </summary>
    public static class Utils
    {
        internal static int Clamp(int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        internal static double Clamp(double value, double min, double max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        internal static bool Equals(double x, double y, double precision = 1E-5)
        {
            return Math.Abs(x - y) < precision;
        }

        /// <summary>
        /// Linearly interpolates between the two colors and returns the resulting color.
        /// </summary>
        public static Color Lerp(this Color color1, Color color2, double amount)
        {
            double sr = color1.R, sg = color1.G, sb = color1.B, sa = color1.A;
            double er = color2.R, eg = color2.G, eb = color2.B, ea = color2.A;

            byte r = (byte)Lerp(sr, er, amount),
                 g = (byte)Lerp(sg, eg, amount),
                 b = (byte)Lerp(sb, eb, amount),
                 a = (byte)Lerp(sa, ea, amount);

            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Linearly interpolates between the two values and returns the result.
        /// </summary>
        public static double Lerp(this double start, double end, double amount)
        {
            double difference = end - start;
            double adjusted = difference * amount;
            return start + adjusted;
        }

        /// <summary>
        /// Linearly interpolates between the colors of the rainbow. 
        /// </summary>
        public static Color Rainbow(double amount)
        {
            double div = (Math.Abs(amount % 1.0) * 6.0);
            byte ascending = (byte)((div % 1) * 255);
            byte descending = (byte)(255 - ascending);

            switch ((int)div)
            {
                case 0:
                    return Color.FromArgb(255, 255, ascending, 0);
                case 1:
                    return Color.FromArgb(255, descending, 255, 0);
                case 2:
                    return Color.FromArgb(255, 0, 255, ascending);
                case 3:
                    return Color.FromArgb(255, 0, descending, 255);
                case 4:
                    return Color.FromArgb(255, ascending, 0, 255);
                case 5:
                default:
                    return Color.FromArgb(255, 255, 0, descending);
            }
        }
    }
}
