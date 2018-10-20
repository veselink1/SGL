using System;

namespace SGL
{
    /// <summary>
    /// Represent an ellipse defined by an origin point and two radii (along the X and Y axes).
    /// </summary>
    public class Ellipse : IEquatable<Ellipse>
    {
        /// <summary>
        /// The center of the ellipse.
        /// </summary>
        public Point Center
        {
            get => new Point(X, Y);
            set { X = value.X; Y = value.Y; }
        }
        /// <summary>
        /// The X-coordinate of the center of the ellipse.
        /// </summary>
        public double X { get; set; }
        /// <summary>
        /// The Y-coordinate of the center of the ellipse.
        /// </summary>
        public double Y { get; set; }
        /// <summary>
        /// The radius along the X-axis.
        /// </summary>
        public double RX { get; set; }
        /// <summary>
        /// The radius along the Y-axis.
        /// </summary>
        public double RY { get; set; }
        /// <summary>
        /// The area of the ellipse.
        /// </summary>
        public double Area => Math.PI * RX * RY;
        /// <summary>
        /// The outer perimeter of the ellipse. Synonymous with <see cref="Circumference"/>.
        /// </summary>
        public double Perimeter => Circumference;
        /// <summary>
        /// The outer perimeter of the ellipse. 
        /// </summary>
        public double Circumference
        {
            get
            {
                if (Utils.Equals(RX, RY))
                {
                    return 2 * Math.PI * RX;
                }
                else
                {
                    return CalculatePerimeter(RX, RY);
                }
            }
        }

        public Ellipse(double x, double y, double rx, double ry)
        {
            X = x;
            Y = y;
            RX = rx;
            RY = ry;
        }

        public Ellipse(double x, double y, double radius)
        {
            X = x;
            Y = y;
            RX = radius;
            RY = radius;
        }

        public Ellipse(Point p, double width, double height)
        {
            X = p.X;
            Y = p.Y;
            RX = width;
            RY = height;
        }
        
        public Ellipse(Point p, double radius)
        {
            X = p.X;
            Y = p.Y;
            RX = radius;
            RY = radius;
        }

        /// <summary>
        /// Checks whether the ellipse contains the point.
        /// </summary>
        public bool Contains(Point p)
        {
            return Math.Pow((X - p.X), 2) / (RX * RX) + Math.Pow((Y - p.Y), 2) / (RY * RY) <= 1;
        }

        public bool Equals(Ellipse other)
        {
            return Center == other.Center && RX == other.RX && RY == other.RY;
        }

        public override bool Equals(object obj)
        {
            return obj is Ellipse e && Equals(e);
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Center.GetHashCode();
            hashCode = hashCode * -1521134295 + RX.GetHashCode();
            hashCode = hashCode * -1521134295 + RY.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Utils.Equals(RX, RY)
                ? $"Circle(Center = {Center}, Radius = {RX})"
                : $"Ellipse(Center = {Center}, RadiusX = {RX}, RadiusY = {RY})";
        }

        public static bool operator ==(Ellipse a, Ellipse b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Ellipse a, Ellipse b)
        {
            return !a.Equals(b);
        }

        private static double CalculatePerimeter(double rx, double ry)
        {
            double diff = rx - ry;
            double sum = rx + ry;
            double h = diff * diff / (sum * sum);
            double p = Math.PI * sum * (1 + 0.25 * h + 0.015625 * h * h + 0.00390625 * h * h * h);
            return p;
        }
    }
}
