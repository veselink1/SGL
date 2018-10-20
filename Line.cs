using System;

namespace SGL
{
    /// <summary>
    /// Represent a line defined by two points.
    /// </summary>
    public class Line : IEquatable<Line>
    {
        /// <summary>
        /// The starting point of the line.
        /// </summary>
        public Point Start
        {
            get => new Point(X1, Y1);
            set { X1 = value.X; Y1 = value.Y; }
        }
        /// <summary>
        /// The ending point of the line.
        /// </summary>
        public Point End
        {
            get => new Point(X2, Y2);
            set { X2 = value.X; Y2 = value.Y; }
        }
        /// <summary>
        /// The X coordinate of the starting point.
        /// </summary>
        public double X1 { get; set; }
        /// <summary>
        /// The X coordinate of the ending point.
        /// </summary>
        public double X2 { get; set; }
        /// <summary>
        /// The Y coordinate of the starting point.
        /// </summary>
        public double Y1 { get; set; }
        /// <summary>
        /// The Y coordinate of the ending point.
        /// </summary>
        public double Y2 { get; set; }
        /// <summary>
        /// Calculates the distance between the two ends of the line.
        /// </summary>
        public double Length => Point.Distance(X1, Y1, X2, Y2);

        /// <summary>
        /// Creates a line between the two points.
        /// </summary>
        public Line(double x1, double y1, double x2, double y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }

        /// <summary>
        /// Creates a line between the two points.
        /// </summary>
        public Line(Point a, Point b)
        {
            X1 = a.X;
            Y1 = a.Y;
            X2 = b.X;
            Y2 = b.Y;
        }

        public bool Equals(Line other)
        {
            return Start == other.Start && End == other.End;
        }

        public override bool Equals(object obj)
        {
            return obj is Line p && Equals(p);
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Start.GetHashCode();
            hashCode = hashCode * -1521134295 + End.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"Line(A = {Start}, B = {End})";
        }

        public static bool operator ==(Line a, Line b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Line a, Line b)
        {
            return !a.Equals(b);
        }
    }
}
