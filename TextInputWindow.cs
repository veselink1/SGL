using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SGL
{
    internal class TextInputWindow : Window
    {
        private TextBox _textBox;
        public string Text => _textBox.Text;
        public bool ClosedNormally { get; private set; }

        internal TextInputWindow(Application app, string message, string initialValue)
        {
            Title = app.MainWindow.Title;
            Owner = app.MainWindow;
            Owner.Closing += OnOwnerClosing;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Width = 320;
            SizeToContent = SizeToContent.Height;

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            var textBlock = new TextBlock
            {
                Text = message,
                FontSize = 15,
                Padding = new Thickness(15, 10, 15, 10),
            };
            Grid.SetRow(textBlock, 0);
            grid.Children.Add(textBlock);
            _textBox = new TextBox
            {
                Text = initialValue,
                FontSize = 15,
                FontFamily = new FontFamily("Courier New"),
                Padding = new Thickness(10),
            };
            Grid.SetRow(_textBox, 1);
            grid.Children.Add(_textBox);
            var btn = new Button
            {
                Content = "OK",
                Padding = new Thickness(8),
                BorderThickness = new Thickness(2),
                IsDefault = true,
            };
            btn.Click += delegate
            {
                ClosedNormally = true;
                Close();
            };
            Grid.SetRow(btn, 2);
            grid.Children.Add(btn);

            Content = grid;
            Show();
            _textBox.Focus();
            _textBox.SelectAll();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            NativeMethods.HideCloseButton(this);
        }

        private void OnOwnerClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
