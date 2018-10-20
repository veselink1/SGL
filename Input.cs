using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace SGL
{
    internal static class Input
    {
        public static string Prompt(Application app, string message, string initialValue = "")
        {
            var tcs = new TaskCompletionSource<string>();
            app.Dispatcher.Invoke(() =>
            {
                if (app.MainWindow == null)
                {
                    throw new InvalidOperationException("The application window is not open!");
                }
                var window = new TextInputWindow(app, message, initialValue);
                window.Closed += delegate(object sender, EventArgs e)
                {
                    var win = (TextInputWindow)sender;
                    if (win.ClosedNormally)
                    {
                        tcs.TrySetResult(win.Text);
                    }
                    else
                    {
                        tcs.TrySetException(new OperationCanceledException("The window was closed abnormally!"));
                    }
                };
            });
            return tcs.Task.Result;
        }

        public static bool YesOrNo(Application app, string message, bool defaultValue = true)
        {
            return SelectOne(app, message, new[] { "Yes", "No" }, defaultValue ? 0 : 1) == 0;
        }

        public static int SelectOne(Application app, string message, object[] options, int defaultValue = -1)
        {
            return SelectOneImpl(app, message, options, defaultValue);
        }

        private static int SelectOneImpl(Application app, string message, object[] options, int defaultValue)
        {
            var tcs = new TaskCompletionSource<int>();
            app.Dispatcher.Invoke(() =>
            {
                if (app.MainWindow == null)
                {
                    throw new InvalidOperationException("The application window is not open!");
                }
                var window = new GridInputWindow(app, message, options, defaultValue);
                window.Closed += delegate(object sender, EventArgs e)
                {
                    var win = (GridInputWindow)sender;
                    if (win.ClosedNormally)
                    {
                        tcs.TrySetResult(win.SelectedOptionIndex);
                    }
                    else
                    {
                        tcs.TrySetException(new OperationCanceledException("The window was closed abnormally!"));
                    }
                };
            });
            return tcs.Task.Result;
        }
    }
}
