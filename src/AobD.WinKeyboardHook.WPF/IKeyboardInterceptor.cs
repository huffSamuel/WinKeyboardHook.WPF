//
// Authors:
//   Samuel Huff contact@samueljhuff.com
//   Lucas Ontivero lucasontivero@gmail.com
//
// Copyright (C) 2018 Samuel Huff
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Inspired by and adapted from the work of Lucas Ontivero (https://github.com/lontivero)

using System;
using System.Windows.Input;

namespace AobD.WinKeyboardHook.WPF
{
    public interface IKeyboardInterceptor
    {
        /// <summary>
        /// Begin capturing keyboard input.
        /// </summary>
        void StartCapturing();

        /// <summary>
        /// Stop capturing keyboard input.
        /// </summary>
        void StopCapturing();

        /// <summary>
        /// Disables repeated events when a key is held.
        /// </summary>
        bool DisableRepeat { get; set; }

        /// <summary>
        /// Prevents the keyboard event from continuing to the OS layer.
        /// </summary>
        bool SuppressWindowsHandling { get; set; }

        /// <summary>
        /// Occurs when a key is pressed while the control has focus
        /// </summary>
        event EventHandler<KeyEventArgs> KeyDown;

        /// <summary>
        /// Occurs when a key is released while the control has focus.
        /// </summary>
        event EventHandler<KeyEventArgs> KeyUp;

        /// <summary>
        /// Occurs when a character. space or backspace key is pressed while the control has focus
        /// </summary>
        event EventHandler<KeyEventArgs> KeyPress;
    }
}
