using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace SGL
{
    /// <summary>
    /// A graphical chart that is shown in a separate window.
    /// </summary>
    public class Graph
    {
        /// <summary>
        /// Encapsulates the current configuration of the graph.
        /// Used when communicating with the UI thread to alleviate the need for synchronization. 
        /// </summary>
        private class GraphConfig
        {
            public SolidColorBrush Stroke { get; set; }
            public SolidColorBrush Fill { get; set; }
            public double Thickness { get; set; }
            public double FontSize { get; set; }
        }

        /// <summary>
        /// The maximum frame-rate that can be set.
        /// </summary>
        private const int MaxFPS = 60;

        /// <summary>
        /// The delta value multiplier is used when evaluating the values
        /// of a function at a certain point. Lower number means higher 
        /// precision but more calculations.
        /// </summary>
        private const int DeltaValueMultiplier = 2;

        /// <summary>
        /// The dispatcher associated with the UI thread of the graph.
        /// </summary>
        private Dispatcher _uiDispatcher;
        /// <summary>
        /// The UI thread spawned for this graph.
        /// </summary>
        private Thread _uiThread;
        private Application _application;
        /// <summary>
        /// A reference to the window. Should only be mutated through the UI thread.
        /// </summary>
        private Window _window;
        /// <summary>
        /// A reference to the canvas element. Should only be mutated through the UI thread.
        /// </summary>
        private Canvas _canvas;
        /// <summary>
        /// The width of the window as set by the user.
        /// </summary>
        private double _width;
        /// <summary>
        /// The height of the window as set by the user.
        /// </summary>
        private double _height;
        /// <summary>
        /// The actual width of the canvas element in DPI-independent pixels.
        /// </summary>
        private double _actualWidth;
        /// <summary>
        /// The actual height of the canvas element in DPI-independent pixels.
        /// </summary>
        private double _actualHeight;
        /// <summary>
        /// The maximum value on the horizontal axis that can be visualised.
        /// </summary>
        private double _xMax;
        /// <summary>
        /// The maximum value on the vertical axis that can be visualised.
        /// </summary>
        private double _yMax;
        /// <summary>
        /// The value that an X-coordinate has to be multiplied by 
        /// to be transformed to window coordinates.
        /// </summary>
        private double XMultiplier => _actualWidth / 2 / _xMax;
        /// <summary>
        /// The value that an Y-coordinate has to be multiplied by 
        /// to be transformed to window coordinates.
        /// </summary>
        private double YMultiplier => _actualHeight / 2 / _yMax;
        /// <summary>
        /// The brush of the strokes made on the canvas.
        /// </summary>
        private SolidColorBrush _strokeBrush;
        /// <summary>
        /// The fill brush of the closed shapes that can be drawn.
        /// </summary>
        private SolidColorBrush _fillBrush;
        /// <summary>
        /// The thickness of the strokes.
        /// </summary>
        private double _thickness;
        /// <summary>
        /// The background color of the graph.
        /// </summary>
        private SolidColorBrush _backgroundBrush;
        /// <summary>
        /// The font size scaling factor. 
        /// </summary>
        private double _fontSizeScale;
        /// <summary>
        /// The target frame-rate set by the user.
        /// </summary>
        private int _targetFramerate;
        /// <summary>
        /// The time it took for the last frame to render.
        /// </summary>
        private double _deltaTime;
        /// <summary>
        /// The total time it took for all frame from the opening of the window to render.
        /// </summary>
        private double _time;
        /// <summary>
        /// The thread-safe render queue.
        /// </summary>
        private ConcurrentQueue<Action> _renderQueue;
        /// <summary>
        /// Invoked when a UI element's rendering event occurs.
        /// </summary>
        private event Action Rendering;

        /// <summary>
        /// The color of the strokes drawn onto the chart.
        /// </summary>
        public Color StrokeColor
        {
            get => _strokeBrush.Color;
            set
            {
                var brush = new SolidColorBrush(value);
                brush.Freeze();
                _strokeBrush = brush;
            }
        }

        /// <summary>
        /// The fill color used when drawing Rectangle and Ellipse
        /// </summary>
        public Color ShapeFill
        {
            get => _fillBrush.Color;
            set
            {
                var brush = new SolidColorBrush(value);
                brush.Freeze();
                _fillBrush = brush;
            }
        }

        /// <summary>
        /// The background color of the window.
        /// </summary>
        public Color Background
        {
            get => _backgroundBrush.Color;
            set
            {
                _backgroundBrush = new SolidColorBrush(value);
                _backgroundBrush.Freeze();
                _uiDispatcher.InvokeAsync(() =>
                {
                    _window.Background = _backgroundBrush;
                });
            }
        }

        /// <summary>
        /// The thickness of the strokes drawn onto the window.
        /// </summary>
        public double StrokeThickness
        {
            get => _thickness;
            set => _thickness = Math.Abs(value);
        }

        /// <summary>
        /// The scale of the size of the font of the labels.
        /// </summary>
        public double FontSize
        {
            get => _fontSizeScale;
            set => _fontSizeScale = Math.Max(0.05, value);
        }

        /// <summary>
        /// The target framerate of the animations. Clamped between 1 and 60.
        /// </summary>
        public int TargetFPS
        {
            get => _targetFramerate;
            set => _targetFramerate = Utils.Clamp(value, 1, 60);
        }

        /// <summary>
        /// A multiplier that can be used to create framerate-independent animations.
        /// </summary>
        /// \code
        /// // Example usage
        /// Graph g = new Graph(5);
        /// Point pt = new Point(0, 1);
        /// while (true) 
        /// {
        ///     // Erase the previously-drawn point (if any).
        ///     g.Erase(pt);
        ///     // Moves the point upwards by 1 unit per seconds (regardless of framerate)
        ///     pt += 2 * Point.Up * g.DeltaTime;
        ///     // Draws the new point.
        ///     g.Draw(pt);
        ///     // Waits for the screen to update (1 frame).
        ///     g.WaitForUpdate();
        /// }
        /// \endcode
        public double DeltaTime => _deltaTime;

        /// <summary>
        /// A framerate-independent time since the window was first shown. 
        /// The aggregate of all previous delta times from their respective frames.
        /// This value is not influenced by pauses like WaitForSeconds() or GetInt() and can
        /// therefore be used to create continuous animations regardless of such interruptions.
        /// </summary>
        /// \code
        /// // Example usage
        /// double prevTime = 0;
        /// while (true) 
        /// {   
        ///     // Will draw the graph of Sin(x) that will
        ///     // be displaced every frame by a given value 
        ///     // and create a wave-like effect.
        ///     g.Draw(x => Math.Sin(x + g.Time);
        ///     // Erase the previous such graph so that the two don't overlap. 
        ///     g.Erase(x => Math.Sin(x + prevTime);
        ///     // Save the latest value of this parameter.
        ///     prevTime = g.Time;
        ///     // Wait for the screen to update.
        ///     g.WaitForUpdate();
        /// }
        /// \endcode
        public double Time => _time;

        /// <summary>
        /// The minimum range of values that can be represented on both Ox-> or Oy->.
        /// </summary>
        public double Range => Math.Min(_xMax, _yMax);

        /// <summary>
        /// Whether the user has requested the window to be closed.
        /// </summary>
        public bool Closed { get; private set; } = false;

        /// <summary>
        /// Creates a new empty chart window.
        /// </summary>
        /// <param name="title">The title of the window.</param>
        /// <param name="range">The minimum value that can be represented on the chart.</param>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        public Graph(double range, string title = "Main Window", double width = 800, double height = 640)
        {
            _width = width;
            _height = height;
            _thickness = 1.0;
            _fontSizeScale = 1.0;
            _targetFramerate = 30;
            _deltaTime = 1.0;
            _strokeBrush = Brushes.DarkCyan;
            _backgroundBrush = Brushes.White;
            _fillBrush = Brushes.Transparent;
            _renderQueue = new ConcurrentQueue<Action>();

            InitializeUI(range, title, width, height);
        }

        /// <summary>
        /// Initializes the UI thread, creates the window and shows it.
        /// </summary>
        private void InitializeUI(double range, string title, double width, double height)
        {
            var tcs = new TaskCompletionSource<object>();
            _uiThread = new Thread(() =>
            {
                try
                {
                    _uiDispatcher = Dispatcher.CurrentDispatcher;
                    _application = new Application();

                    _window = new Window
                    {
                        Title = title,
                        Width = width,
                        Height = height,
                        ResizeMode = ResizeMode.CanMinimize,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    };

                    var canvasTransform = new ScaleTransform(1.0, 1.0);
                    _canvas = new Canvas
                    {
                        LayoutTransform = canvasTransform,
                    };

                    _window.Content = _canvas;
                    _window.MouseWheel += OnMouseWheel;
                    _window.ContentRendered += delegate
                    {
                            // Save the actual (WPF) dimensions of the canvas element.
                            _actualWidth = _canvas.ActualWidth;
                        _actualHeight = _canvas.ActualHeight;
                            // Set the maximum X and Y values that can be depicted on the canvas according to the range
                            // set by the user.
                            _xMax = _actualWidth <= _actualHeight ? range : _actualWidth / _actualHeight * range;
                        _yMax = _actualWidth >= _actualHeight ? range : _actualHeight / _actualWidth * range;
                        tcs.SetResult(null);
                    };
                    _window.Closing += delegate
                    {
                        Closed = true;
                        _application.Shutdown(0);
                    };

                    CompositionTarget.Rendering += OnCompositionTargetRendering;
                    _application.Run(_window);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });

            _uiThread.SetApartmentState(ApartmentState.STA);
            _uiThread.Start();

            try
            {
                tcs.Task.Wait();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.StackTrace, "Failed to initialize the graph window.", MessageBoxButton.OK);
            }
        }

        private void OnCompositionTargetRendering(object sender, EventArgs e)
        {
            _uiDispatcher.InvokeAsync(() =>
            {
                ExecuteRenderQueue();
                Rendering?.Invoke();
            }, DispatcherPriority.Input);
        }

        private void ExecuteRenderQueue()
        {
            while (_renderQueue.TryDequeue(out Action result))
            {
                result();
            }
        }

        /// <summary>
        /// Shows the console window.
        /// </summary>
        public static void ShowConsole()
        {
            IntPtr hConsole = NativeMethods.GetConsoleWindow();
            if (hConsole != IntPtr.Zero)
            {
                NativeMethods.ShowWindow(hConsole, NativeMethods.SW_SHOW);
            }
        }

        /// <summary>
        /// Hides the console window.
        /// </summary>
        public static void HideConsole()
        {
            IntPtr hConsole = NativeMethods.GetConsoleWindow();
            if (hConsole != IntPtr.Zero)
            {
                NativeMethods.ShowWindow(hConsole, NativeMethods.SW_HIDE);
            }
        }

        /// <summary>
        /// Checks if the specified key is currently pressed.
        /// </summary>
        public bool IsKeyDown(Key key)
        {
            return _uiDispatcher.Invoke(() => Keyboard.IsKeyDown(key), DispatcherPriority.Send);
        }

        /// <summary>
        /// Checks if the specified mouse button is currently pressed.
        /// </summary>
        public bool IsMouseButtonDown(MouseButton button)
        {
            return _uiDispatcher.Invoke(() =>
            {
                switch (button)
                {
                    case MouseButton.Left:
                        return Mouse.LeftButton == MouseButtonState.Pressed;
                    case MouseButton.Right:
                        return Mouse.RightButton == MouseButtonState.Pressed;
                    case MouseButton.Middle:
                        return Mouse.MiddleButton == MouseButtonState.Pressed;
                    case MouseButton.XButton1:
                        return Mouse.XButton1 == MouseButtonState.Pressed;
                    case MouseButton.XButton2:
                        return Mouse.XButton2 == MouseButtonState.Pressed;
                }
                return false;
            }, DispatcherPriority.Send);
        }

        /// <summary>
        /// Asks the user to select a point on the chart and returns the coordinates of that point.
        /// </summary>
        public Point SelectPoint()
        {
            var tcs = new TaskCompletionSource<Point>();
            _uiDispatcher.InvokeAsync(() =>
            {
                // Save the original title for later so that it can be restored. 
                // Note that the user won't be able to change the title of the window
                // during this function's invocation, as this function works in a 
                // blocking manner (as do all other function that are part of the public interface)
                string originalTitle = _window.Title;
                Mouse.OverrideCursor = Cursors.Pen;

                Point mousePt = default(Point);
                MouseEventHandler onMove = delegate (object source, MouseEventArgs e)
                {
                    var mediaPt = e.GetPosition(_canvas);
                    // Transform the coordinates to virtual coordinates.
                    mousePt = new Point(mediaPt.X / _actualWidth * _xMax * 2 - _xMax, _yMax - mediaPt.Y / _actualHeight * _yMax * 2);
                    // Update the title of the window.
                    _window.Title = originalTitle + $" • Waiting for input ({mousePt.X.ToString("0.000")}, {mousePt.Y.ToString("0.000")})";
                };
                MouseButtonEventHandler onClick = null;
                onClick = delegate
                {
                    // Capture the current position of the mouse
                    // remove the listeners and restore the title.
                    tcs.TrySetResult(mousePt);
                    _window.MouseMove -= onMove;
                    _window.MouseLeftButtonDown -= onClick;
                    _window.Title = originalTitle;
                };
                _window.MouseMove += onMove;
                _window.MouseLeftButtonDown += onClick;
            });

            Point p = default(Point);
            try
            {
                p = tcs.Task.Result;
            }
            finally
            {
                // Always restore the mouse cursor to its default state.
                _uiDispatcher.InvokeAsync(() => Mouse.OverrideCursor = null);
            }
            return p;
        }

        /// <summary>
        /// Bound to the mouse wheel event. Scales the window accordingly to the user's scrolling of the wheel.
        /// </summary>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var delta = e.Delta / 120.0;
            var multiplier = 1.0 + delta * 0.05;

            var newWidth = _canvas.ActualWidth * multiplier + (_window.Width - _canvas.ActualWidth);
            var newHeight = _canvas.ActualHeight * multiplier + (_window.Height - _canvas.ActualHeight);

            bool windowTooSmall = newWidth <= 320 || newHeight <= 240;
            bool windowTooBig = SystemParameters.WorkArea.Width <= newWidth || SystemParameters.WorkArea.Height <= newHeight;
            if (windowTooSmall || windowTooBig)
            {
                return;
            }

            _window.Width = newWidth;
            _window.Height = newHeight;
            var scaleTransform = _canvas.LayoutTransform as ScaleTransform;
            scaleTransform.ScaleX += delta * 0.05;
            scaleTransform.ScaleY += delta * 0.05;
        }

        /// <summary>
        /// Shows the window.
        /// </summary>
        public void Show()
        {
            _uiDispatcher.Invoke(() => _window.Show());
        }

        /// <summary>
        /// Hides the window.
        /// </summary>
        public void Hide()
        {
            _uiDispatcher.Invoke(() => _window.Hide());
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        public void Close()
        {
            _uiDispatcher.Invoke(() => _window.Close());
        }

        /// <summary>
        /// Clears the content of the chart.
        /// </summary>
        public void Clear()
        {
            AddToRenderQueue(dc => _canvas.Children.Clear());
        }

        /// <summary>
        /// Waits for the user to manually close the window.
        /// </summary>
        public void WaitForExit()
        {
            _uiThread.Join();
        }

        /// <summary>
        /// Reads a 32-bit integer from the user through a graphical prompt.
        /// </summary>
        public int ReadInt(string message = "Please, enter an integer.", int defaultValue = 0)
        {
            string input = Input.Prompt(_application, message, defaultValue.ToString());
            int result;
            while (!int.TryParse(input, out result))
            {
                input = Input.Prompt(_application, message, defaultValue.ToString());
            }
            return result;
        }

        /// <summary>
        /// Reads a 64-bit integer from the user through a graphical prompt.
        /// </summary>
        public long ReadLong(string message = "Please, enter an integer.", long defaultValue = 0)
        {
            string input = Input.Prompt(_application, message, defaultValue.ToString());
            long result;
            while (!long.TryParse(input, out result))
            {
                input = Input.Prompt(_application, message, defaultValue.ToString());
            }
            return result;
        }

        /// <summary>
        /// Reads a 32-bit floating point number from the user through a graphical prompt.
        /// </summary>
        public float ReadFloat(string message = "Please, enter a number.", float defaultValue = 0f)
        {
            string input = Input.Prompt(_application, message, defaultValue.ToString());
            float result;
            while (!float.TryParse(input, out result))
            {
                input = Input.Prompt(_application, message, defaultValue.ToString());
            }
            return result;
        }

        /// <summary>
        /// Reads a 64-bit floating point number from the user through a graphical prompt.
        /// </summary>
        public double ReadDouble(string message = "Please, enter a number.", double defaultValue = 0.0)
        {
            string input = Input.Prompt(_application, message, defaultValue.ToString());
            double result;
            while (!double.TryParse(input, out result))
            {
                input = Input.Prompt(_application, message, defaultValue.ToString());
            }
            return result;
        }

        /// <summary>
        /// Reads a string from the user through a graphical prompt.
        /// </summary>
        public string ReadString(string message = "Please, enter some text.", string defaultValue = "")
        {
            return Input.Prompt(_application, message, defaultValue);
        }

        /// <summary>
        /// Asks the user a questing and allows them to answer it with yer (true) or no (false) through a graphical prompt.
        /// </summary>
        public bool YesOrNo(string message, bool defaultValue = true)
        {
            return Input.YesOrNo(_application, message, defaultValue);
        }

        /// <summary>
        /// Asks the user to select an item from the list and returns the selected item.
        /// </summary>
        /// \code
        /// // Example usage
        /// Color[] availableColors = new Color[] {
        ///     Colors.Black,
        ///     Colors.Blue,
        ///     Colors.Green,
        /// };
        /// Color selectedColor = g.SelectItem("Choose a background color...", availableColors);
        /// g.Background = selectedColor;
        /// \endcode
        public T SelectItem<T>(string message, T[] options, int defaultIndex = -1)
        {
            int index = Input.SelectOne(_application, message, options.Select(x => (object)x).ToArray(), defaultIndex);
            return options[index];
        }

        /// <summary>
        /// Asks the user to select an item from the list and returns the selected item.
        /// </summary>
        /// \code
        /// // Example usage
        /// Color[] availableColors = new Color[] {
        ///     Colors.Black,
        ///     Colors.Blue,
        ///     Colors.Green,
        /// };
        /// int index = g.SelectIndex("Choose a background color...", availableColors);
        /// Color selectedColor = availableColors[index];
        /// g.Background = selectedColor;
        /// \endcode
        public int SelectIndex<T>(string message, T[] options, int defaultIndex = -1)
        {
            return Input.SelectOne(_application, message, options.Select(x => (object)x).ToArray(), defaultIndex);
        }

        /// <summary>
        /// Shows a message to the user.
        /// </summary>
        public void Notify(string message)
        {
            Input.SelectOne(_application, message, new[] { "OK" }, 0);
        }

        /// <summary>
        /// Adds the graph of a function to the chart. The start and end of the values along the X-axis can also be specified.
        /// </summary>
        public void Draw(Func<double, double> f, double start = double.MinValue, double end = double.MaxValue)
        {
            var curves = Evaluate(f, start, end);
            AddToRenderQueue(dc =>
            {
                foreach (var c in curves)
                {
                    var polyline = new System.Windows.Shapes.Polyline
                    {
                        DataContext = c,
                        Points = c,
                        Stroke = dc.Stroke,
                        StrokeThickness = dc.Thickness,
                    };

                    Canvas.SetLeft(polyline, 0);
                    Canvas.SetTop(polyline, 0);
                    _canvas.Children.Add(polyline);
                }
            });
        }

        /// <summary>
        /// Erases a function that is currently displayed on the graph. The start and end of the values along the X-axis can also be specified.
        /// </summary>
        public void Erase(Func<double, double> f, double start = double.MinValue, double end = double.MaxValue)
        {
            var curves = Evaluate(f, start, end);
            AddToRenderQueue(dc =>
            {
                var toRemove = new List<UIElement>();
                foreach (var c in curves)
                {
                    foreach (var shape in _canvas.Children)
                    {
                        if (shape is System.Windows.Shapes.Polyline polyline)
                        {
                            if (c.Count == polyline.Points.Count && c.SequenceEqual(polyline.Points))
                            {
                                toRemove.Add(polyline);
                            }
                        }
                    }
                }
                foreach (var x in toRemove)
                {
                    _canvas.Children.Remove(x);
                }
            });
        }

        private List<PointCollection> Evaluate(Func<double, double> f, double start, double end)
        {
            if (start > end)
            {
                double c = start;
                start = end;
                end = start;
            }

            start = Math.Max(-_xMax, start);
            end = Math.Min(_xMax, end);

            var viewport = new Rectangle(Point.Origin, _xMax * 2, _yMax * 2);

            // The change in X for all evaluated points.
            // Multiplied by a constant to reduce processor strain.
            double dx = DeltaValueMultiplier * _xMax / _actualWidth;
            // The total number of points that will be considered when graphing the function.
            int npts = (int)Math.Ceiling((end - start) / dx);

            var curves = new List<PointCollection>();
            var curve = new PointCollection(npts);

            var pts = new List<Point>(npts);
            for (int i = 0; i < npts; i++)
            {
                double x1 = start + dx * i;
                double y1 = f(x1);
                // Do not add the point if the y-value is NaN.
                if (!double.IsNaN(y1))
                {
                    pts.Add(new Point(x1, y1));
                }
            }

            // Update the number of points to the
            // actual count, since some of the points might have 
            // turned out to have NaN as y-value and were consequently skipped.
            npts = pts.Count;

            for (int i = 0; i < npts; i++)
            {
                var pt = pts[i];
                bool prevInViewport = i > 1 && viewport.Contains(pts[i - 1]);
                bool nextInViewport = i < npts - 1 && viewport.Contains(pts[i + 1]);
                // Skip the points if they are not visible
                // and divide the graph into separate curves.
                if (viewport.Contains(pt) || prevInViewport || nextInViewport)
                {
                    curve.Add(new System.Windows.Point
                    {
                        X = TransformX(pt.X),
                        Y = TransformY(pt.Y),
                    });
                }
                else if (curve.Count > 0)
                {
                    curves.Add(curve);
                    curve = new PointCollection();
                }
            }
            curves.Add(curve);
            foreach (var c in curves)
            {
                c.Freeze();
            }

            return curves;
        }

        /// <summary>
        /// Adds the X and Y axes to the graph.
        /// </summary>
        public void DrawAxes()
        {
            Draw(new Line(-_xMax, 0, _xMax, 0));
            Draw(new Line(0, -_yMax, 0, _yMax));
        }

        /// <summary>
        /// Adds a line of text to the chart.
        /// </summary>
        public void Draw(Label t)
        {
            AddToRenderQueue(dc => AddToCanvas(t, dc));
        }

        /// <summary>
        /// Removes a currently visible line of text from the chart.
        /// </summary>
        public void Erase(Label t)
        {
            AddToRenderQueue(dc => RemoveFromCanvas(t));
        }

        /// <summary>
        /// Adds a line to the chart.
        /// </summary>
        public void Draw(Line l)
        {
            AddToRenderQueue(dc => AddToCanvas(l, dc));
        }

        /// <summary>
        /// Removes a currently visible line from the chart.
        /// </summary>
        public void Erase(Line l)
        {
            AddToRenderQueue(dc => RemoveFromCanvas(l));
        }

        /// <summary>
        /// Adds a rectangle to the chart.
        /// </summary>
        public void Draw(Rectangle r)
        {
            AddToRenderQueue(dc => AddToCanvas(r, dc));
        }

        /// <summary>
        /// Removes a currently visible rectangle from the chart.
        /// </summary>
        public void Erase(Rectangle r)
        {
            AddToRenderQueue(dc => RemoveFromCanvas(r));
        }

        /// <summary>
        /// Adds a point to the chart.
        /// </summary>
        public void Draw(Point p)
        {
            AddToRenderQueue(dc => AddToCanvas(p, dc));
        }

        /// <summary>
        /// Removes a currently visible point from the chart.
        /// </summary>
        public void Erase(Point p)
        {
            AddToRenderQueue(dc => RemoveFromCanvas(p));
        }

        /// <summary>
        /// Adds an ellipse to the chart.
        /// </summary>
        public void Draw(Ellipse e)
        {
            AddToRenderQueue(dc => AddToCanvas(e, dc));
        }

        /// <summary>
        /// Removes an ellipse from the chart.
        /// </summary>
        public void Erase(Ellipse e)
        {
            AddToRenderQueue(dc => RemoveFromCanvas(e));
        }

        /// <summary>
        /// Waits a number of seconds.
        /// </summary>
        public void WaitForSeconds(double seconds)
        {
            if (seconds > 0)
            {
                Thread.Sleep((int)(seconds * 1000.0));
            }
        }

        /// <summary>
        /// Waits for the CompositionTarget.Rendering event to occur.
        /// </summary>
        /// <param name="timeout"></param>
        private void WaitForRendering(int timeout)
        {
            var tcs = new TaskCompletionSource<object>();
            Action action = null;
            action = () => tcs.TrySetResult(null);
            Rendering += action;
            try
            {
                tcs.Task.Wait(timeout);
            }
            finally
            {
                Rendering -= action;
            }
        }

        /// <summary>
        /// Waits for the screen to be updates. (Waits a single frame.)
        /// </summary>
        public void WaitForUpdate()
        {
            int targetFrameTime = (int)(1000.0 / _targetFramerate);
            var st = new Stopwatch();
            st.Start();
            WaitForRendering(targetFrameTime);
            var additionalSleep = targetFrameTime - (int)st.ElapsedMilliseconds;
            if (additionalSleep > 0)
            {
                Thread.Sleep(additionalSleep);
            }
            st.Stop();
            _deltaTime = st.ElapsedMilliseconds / 1000.0;
            _time += _deltaTime;
        }

        /// <summary>
        /// Adds a label to the canvas. Must be run on the UI thread and should not be called directly.
        /// </summary>
        private void AddToCanvas(Label t, GraphConfig dc)
        {
            var s = new TextBlock
            {
                DataContext = t,
                Foreground = dc.Stroke,
                Text = t.Text,
                FontSize = 100.0 / _xMax * dc.FontSize,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Courier New")
            };

            s.Measure(new Size(_canvas.ActualWidth, _canvas.ActualHeight));

            double offsetX = 0;
            double offsetY = 0;
            switch (t.Alignment)
            {
                case Alignment.Bottom:
                    offsetY = s.DesiredSize.Height / 2;
                    break;
                case Alignment.Top:
                    offsetY = -s.DesiredSize.Height / 2;
                    break;
                case Alignment.Left:
                    offsetX = -s.DesiredSize.Width / 2;
                    break;
                case Alignment.Right:
                    offsetX = s.DesiredSize.Width / 2;
                    break;
            }

            Canvas.SetLeft(s, TransformX(t.Position.X) - s.DesiredSize.Width / 2 + offsetX);
            Canvas.SetTop(s, TransformY(t.Position.Y) - s.DesiredSize.Height / 2 + offsetY);
            _canvas.Children.Add(s);
        }

        /// <summary>
        /// Adds a line to the canvas. Must be run on the UI thread and should not be called directly.
        /// </summary>
        private void AddToCanvas(Line l, GraphConfig dc)
        {
            var s = new System.Windows.Shapes.Line
            {
                DataContext = l,
                X1 = TransformX(l.X1),
                Y1 = TransformY(l.Y1),
                X2 = TransformX(l.X2),
                Y2 = TransformY(l.Y2),
                StrokeThickness = dc.Thickness,
                Stroke = dc.Stroke,
                Fill = dc.Fill,
            };

            _canvas.Children.Add(s);
        }

        /// <summary>
        /// Adds a rectangle to the canvas. Must be run on the UI thread and should not be called directly.
        /// </summary>
        private void AddToCanvas(Rectangle r, GraphConfig dc)
        {
            var w = r.Width * XMultiplier;
            var h = r.Height * YMultiplier;
            var s = new System.Windows.Shapes.Rectangle
            {
                DataContext = r,
                Width = w,
                Height = h,
                StrokeThickness = dc.Thickness,
                Stroke = dc.Stroke,
                Fill = dc.Fill,
            };

            Canvas.SetLeft(s, TransformX(r.Left));
            Canvas.SetTop(s, TransformY(r.Top));
            _canvas.Children.Add(s);
        }

        /// <summary>
        /// Adds a point to the canvas. Must be run on the UI thread and should not be called directly.
        /// </summary>
        private void AddToCanvas(Point p, GraphConfig dc)
        {
            var wh = dc.Thickness * 0.01 * Math.Min(_actualWidth, _actualHeight);
            var s = new System.Windows.Shapes.Ellipse
            {
                DataContext = p,
                Width = wh,
                Height = wh,
                StrokeThickness = dc.Thickness,
                Stroke = dc.Stroke,
                Fill = dc.Stroke,
            };

            Canvas.SetLeft(s, TransformX(p.X) - wh / 2);
            Canvas.SetTop(s, TransformY(p.Y) - wh / 2);
            _canvas.Children.Add(s);
        }

        /// <summary>
        /// Adds an ellipse to the canvas. Must be run on the UI thread and should not be called directly.
        /// </summary>
        private void AddToCanvas(Ellipse e, GraphConfig dc)
        {
            var w = e.RX * _actualWidth / _xMax;
            var h = e.RY * _actualHeight / _yMax;
            var s = new System.Windows.Shapes.Ellipse
            {
                DataContext = e,
                Width = w,
                Height = h,
                StrokeThickness = dc.Thickness,
                Stroke = dc.Stroke,
                Fill = dc.Fill,
            };

            Canvas.SetLeft(s, TransformX(e.X) - w / 2);
            Canvas.SetTop(s, TransformY(e.Y) - h / 2);
            _canvas.Children.Add(s);
        }

        /// <summary>
        /// Removes a shape from the canvas, by comparing it with the DataContext of the actual UIElements added to the canvas.
        /// (All UIElements that depict a certain SGL shape have their DataContext set to that shape).
        /// </summary>
        private void RemoveFromCanvas(object shape)
        {
            var elements = _canvas.Children.Cast<FrameworkElement>().Where(x => x.DataContext.Equals(shape)).ToArray();
            foreach (var elem in elements)
            {
                _canvas.Children.Remove(elem);
            }
        }

        /// <summary>
        /// Invokes the action on the UI thread asynchronously.
        /// </summary>
        private void AddToRenderQueue(Action<GraphConfig> action)
        {
            var dc = new GraphConfig
            {
                Fill = _fillBrush,
                Stroke = _strokeBrush,
                Thickness = _thickness,
                FontSize = _fontSizeScale,
            };
            _renderQueue.Enqueue(() =>
            {
                try
                {
                    action(dc);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + e.StackTrace, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        /// <summary>
        /// Transforms a point to canvas-space.
        /// </summary>
        private Point TransformToCanvas(Point p)
        {
            var left = _actualWidth / 2.0 + p.X * _actualWidth / 2.0 / _xMax;
            var top = _actualHeight / 2.0 - p.Y * _actualHeight / 2.0 / _yMax;
            return new Point(left, top);
        }

        /// <summary>
        /// Transforms an X-value to the X-value on the canvas.
        /// </summary>
        private double TransformX(double x)
        {
            return _actualWidth / 2.0 + x * _actualWidth / 2.0 / _xMax;
        }

        /// <summary>
        /// Transforms an Y-value to the Y-value on the canvas.
        /// </summary>
        private double TransformY(double y)
        {
            return _actualHeight / 2.0 - y * _actualHeight / 2.0 / _yMax;
        }
    }
}
