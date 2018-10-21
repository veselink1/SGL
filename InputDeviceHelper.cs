using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SGL
{
    internal class InputDeviceHelper
    {
        public Dispatcher Dispatcher { get; }

        private readonly object _syncRoot = new object();
        private readonly Dictionary<Key, bool> _kbState;
        private readonly Dictionary<MouseButton, bool> _mbState;

        public InputDeviceHelper(DependencyObject context)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                throw new InvalidOperationException("The calling thread must be STA!");
            }

            Dispatcher = Dispatcher.CurrentDispatcher;

            var keys = Enum.GetValues(typeof(Key)).Cast<Key>().ToArray();
            _kbState = new Dictionary<Key, bool>(keys.Length);
            foreach (var key in keys)
            {
                _kbState[key] = false;
            }

            var mouseButtons = Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>().ToArray();
            _mbState = new Dictionary<MouseButton, bool>(mouseButtons.Length);
            foreach (var button in mouseButtons)
            {
                _mbState[button] = false;
            }

            Keyboard.AddKeyUpHandler(context, OnKeyStateChanged);
            Keyboard.AddKeyDownHandler(context, OnKeyStateChanged);
            Keyboard.AddLostKeyboardFocusHandler(context, OnKeyboardFocusChanged);
            Mouse.AddMouseUpHandler(context, OnMouseButtonStateChanged);
            Mouse.AddMouseDownHandler(context, OnMouseButtonStateChanged);
        }

        private void OnKeyStateChanged(object sender, KeyEventArgs e)
        {
            lock (_syncRoot)
            {
                _kbState[e.Key] = e.IsDown;
            }
        }

        private void OnKeyboardFocusChanged(object sender, KeyboardFocusChangedEventArgs e)
        {
            lock (_syncRoot)
            {
                foreach (var key in _kbState.Keys.ToArray())
                {
                    _kbState[key] = false;
                }
                foreach (var button in _mbState.Keys.ToArray())
                {
                    _mbState[button] = false;
                }
            }
        }

        private void OnMouseButtonStateChanged(object sender, MouseEventArgs e)
        {
            lock (_syncRoot)
            {
                _mbState[MouseButton.Left] = e.LeftButton == MouseButtonState.Pressed;
                _mbState[MouseButton.Right] = e.RightButton == MouseButtonState.Pressed;
                _mbState[MouseButton.Middle] = e.MiddleButton == MouseButtonState.Pressed;
                _mbState[MouseButton.XButton1] = e.XButton1 == MouseButtonState.Pressed;
                _mbState[MouseButton.XButton2] = e.XButton2 == MouseButtonState.Pressed;
            }
        }

        public bool IsKeyDown(Key key)
        {
            lock (_syncRoot)
            {
                return _kbState[key];
            }
        }

        public bool IsMouseButtonDown(MouseButton button)
        {
            lock (_syncRoot)
            {
                return _mbState[button];
            }
        }
    }
}
