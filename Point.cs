using System;

namespace SGL
{
    /// <summary>
    /// Represent a point defined by two coordinates (X and Y).
    /// </summary>
    public struct Point : IEquatable<Point>
    {
        public double X { get; set; }
        public double Y { get; set; }

        /// <summary>
        /// Creates a point with the given coorinates.
        /// </summary>
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// The origin point (0, 0).
        /// </summary>
        public static Point Origin => new Point(0, 0);

        /// <summary>
        /// The point (0, 1).
        /// </summary>
        public static Point Up => new Point(0, 1);

        /// <summary>
        /// The point (0, -1).
        /// </summary>
        public static Point Down => new Point(0, -1);

        /// <summary>
        /// The point (-1, 0).
        /// </summary>
        public static Point Left => new Point(-1, 0);

        /// <summary>
        /// The point (0, 1).
        /// </summary>
        public static Point Right => new Point(1, 0);

        public static Point operator -(Point p)
        {
            return new Point(-p.X, -p.Y);
        }

        public static Point operator+(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        public static Point operator *(Point p, double value)
        {
            return new Point(p.X * value, p.Y * value);
        }

        public static Point operator /(Point p, double value)
        {
            return new Point(p.X / value, p.Y / value);
        }

        /// <summary>
        /// Calculates the distance from one point to another.
        /// </summary>
        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        /// <summary>
        /// Calculates the distance from one point to another.
        /// </summary>
        public double Distance(Point other)
        {
            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }

        /// <summary>
        /// Linearly interpolates between the two points by the specified amount.
        /// Returns <paramref name="value1"/> for 0, <paramref name="value2"/> for 1.
        /// </summary>
        public Point Lerp(Point value2, double amount)
        {
            return new Point(
                X + (value2.X - X) * amount,
                Y + (value2.Y - Y) * amount);
        }

        public bool Equals(Point other)
        {
            return Utils.Equals(X, other.X) && Utils.Equals(Y, other.Y);
        }

        public override bool Equals(object obj)
        {
            return obj is Point p && Equals(p);
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"({X.ToString("0.000")}, {Y.ToString("0.000")})";
        }

        public static bool operator==(Point a, Point b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Point a, Point b)
        {
            return !a.Equals(b);
        }
    }
}
