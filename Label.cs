using System;

namespace SGL
{
    /// <summary>
    /// Represents the position of the label relative the point.
    /// </summary>
    public enum Alignment
    {
        Center,
        Top, 
        Left,
        Right,
        Bottom,
    }

    /// <summary>
    /// Represent some text on the graph, defined by an origin point and a text message.
    /// </summary>
    public class Label : IEquatable<Label>
    {
        /// <summary>
        /// The X-coordinate of the label.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// The Y-coordinate of the label.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// The position of the label.
        /// </summary>
        public Point Position
        {
            get => new Point(X, Y);
            set { X = value.X; Y = value.Y; }
        }

        /// <summary>
        /// The text content of the label.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The alignment of the label relative to the point.
        /// </summary>
        public Alignment Alignment { get; set; }

        /// <summary>
        /// Creates a label at the given position and with the given message and alignment.
        /// </summary>
        public Label(Point position, string text, Alignment alignment = Alignment.Top)
        {
            Position = position;
            Text = text;
            Alignment = alignment;
        }

        /// <summary>
        /// Creates a label at the given position and with the given message and alignment.
        /// </summary>
        public Label(double x, double y, string text, Alignment alignment = Alignment.Top)
        {
            Position = new Point(x, y);
            Text = text;
            Alignment = alignment;
        }

        public bool Equals(Label other)
        {
            return Position == other.Position && Text == other.Text;
        }

        public override bool Equals(object obj)
        {
            return obj is Label l && Equals(l);
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Position.GetHashCode();
            hashCode = hashCode * -1521134295 + Text.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Label a, Label b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Label a, Label b)
        {
            return !a.Equals(b);
        }
    }
}
