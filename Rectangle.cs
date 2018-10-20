using System;

namespace SGL
{
    /// <summary>
    /// Represent a rectangle defined by its center and side dimensions.
    /// </summary>
    public class Rectangle : IEquatable<Rectangle>
    {
        /// <summary>
        /// The center of the rectangle.
        /// </summary>
        public Point Center
        {
            get => new Point((Left + Right) / 2, (Top + Bottom) / 2);
            set { Left = value.X - Width / 2; Top = value.Y - Height / 2; }
        }
        /// <summary>
        /// The X coordinate of the starting point.
        /// </summary>
        public double Left { get; set; }
        /// <summary>
        /// The X coordinate of the ending point.
        /// </summary>
        public double Right { get; set; }
        /// <summary>
        /// The Y coordinate of the starting point.
        /// </summary>
        public double Top { get; set; }
        /// <summary>
        /// The Y coordinate of the ending point.
        /// </summary>
        public double Bottom { get; set; }

        /// <summary>
        /// The width of the rectangle.
        /// </summary>
        public double Width
        {
            get => Math.Abs(Left - Right);
            set { var c = Center; Left = c.X - value / 2; Right = Left + value; }
        }
        /// <summary>
        /// The height of the rectangle.
        /// </summary>
        public double Height
        {
            get => Math.Abs(Top - Bottom);
            set { var c = Center; Top = c.Y + value / 2; Bottom = Top - value; }
        }
        /// <summary>
        /// Calculates the area of the rectangle
        /// </summary>
        public double Area => Width * Height;
        /// <summary>
        /// Calculates the perimeter of the rectangle
        /// </summary>
        public double Perimeter => 2 * (Width + Height);

        public Rectangle(double x, double y, double width, double height)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException("Rectangle(center, width, height): width must be positive", nameof(width));
            }
            if (height < 0)
            {
                throw new ArgumentOutOfRangeException("Rectangle(center, width, height): height must be positive", nameof(height));
            }
            Left = x - width / 2;
            Top = y + height / 2;
            Right = Left + width;
            Bottom = Top - height;
        }
        
        public Rectangle(Point center, double width, double height)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException("Rectangle(center, width, height): width must be positive", nameof(width));
            }
            if (height < 0)
            {
                throw new ArgumentOutOfRangeException("Rectangle(center, width, height): height must be positive", nameof(height));
            }
            Left = center.X - width / 2;
            Top = center.Y + height / 2;
            Right = Left + width;
            Bottom = Top - height;
        }

        public bool Contains(Point p)
        {
            return Left < p.X && Right > p.X
                && Top > p.Y && Bottom < p.Y;
        }

        public bool Equals(Rectangle other)
        {
            return Utils.Equals(Top, other.Top)
                && Utils.Equals(Left, other.Left)
                && Utils.Equals(Bottom, other.Bottom)
                && Utils.Equals(Right, other.Right);
        }

        public override bool Equals(object obj)
        {
            return obj is Rectangle rc && Equals(rc);
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Top.GetHashCode();
            hashCode = hashCode * -1521134295 + Left.GetHashCode();
            hashCode = hashCode * -1521134295 + Bottom.GetHashCode();
            hashCode = hashCode * -1521134295 + Right.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Utils.Equals(Width, Height)
                ? $"Square(Center = {Center}, Side = {Width})"
                : $"Rectangle(Center = {Center}, Width = {Width}, Height = {Height})";
        }

        public static bool operator ==(Rectangle a, Rectangle b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Rectangle a, Rectangle b)
        {
            return !a.Equals(b);
        }
    }
}
