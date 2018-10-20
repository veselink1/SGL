using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SGL
{
    internal class GridInputWindow : System.Windows.Window
    {
        public int SelectedOptionIndex { get; private set; }
        public bool ClosedNormally { get; private set; }

        public GridInputWindow(Application app, string message, object[] options, int defaultOption = -1)
        {
            Title = app.MainWindow.Title;
            Owner = app.MainWindow;
            Owner.Closing += OnOwnerClosing;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            MinWidth = 320;
            MinHeight = 120;
            SizeToContent = SizeToContent.WidthAndHeight;

            double frx = Math.Ceiling(Math.Sqrt(options.Length));
            double fry = Math.Ceiling(options.Length / frx);

            int rows = (int)fry;
            int cols = (int)frx;

            var grid = new Grid
            {
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            for (int i = 0; i < cols; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int i = 0; i < rows; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < options.Length; i++)
            {
                var row = i % rows;
                var col = i / rows;
                var opt = options[i];
                var btn = new Button
                {
                    Content = opt,
                    DataContext = i,
                    BorderThickness = defaultOption == i ? new Thickness(2) : new Thickness(1),
                    IsDefault = defaultOption == i,
                    Padding = defaultOption == i ? new Thickness(8) : new Thickness(9),
                };

                if (opt is System.Windows.Media.Color c)
                {
                    var border = new Border
                    {
                        Child = new TextBlock
                        {
                            Text = opt.ToString(),
                            Foreground = Luminance(c) >= 0.5 ? Brushes.Black : Brushes.White,
                        },
                        Background = new SolidColorBrush(c),
                        Padding = new Thickness(6),
                    };
                    btn.Content = border;
                    btn.BorderThickness = new Thickness(1);
                    btn.Padding = new Thickness(3);
                    btn.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    btn.VerticalContentAlignment = VerticalAlignment.Stretch;
                }

                btn.Click += OnOptionClick;
                Grid.SetColumn(btn, col);
                Grid.SetRow(btn, row);
                grid.Children.Add(btn);
            }

            var container = new Grid();
            container.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            container.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            var text = new TextBlock
            {
                FontSize = 15,
                Padding = new Thickness(15),
                Text = message,
            };
            Grid.SetRow(text, 0);
            container.Children.Add(text);
            Grid.SetRow(grid, 1);
            container.Children.Add(grid);

            Content = container;
            Show();
        }

        private void OnOwnerClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private static double Luminance(System.Windows.Media.Color color)
        {
            return (color.R * 2 + color.B + color.G * 3) / 1536.0;
        }

        private void OnOptionClick(object sender, RoutedEventArgs e)
        {
            SelectedOptionIndex = (int)(sender as Button).DataContext;
            ClosedNormally = true;
            Close();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            NativeMethods.HideCloseButton(this);
        }
    }
}
