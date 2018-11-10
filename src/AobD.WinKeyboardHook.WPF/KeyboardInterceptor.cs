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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace AobD.WinKeyboardHook.WPF
{
    public sealed class KeyboardInterceptor : IKeyboardInterceptor
    {
        #region Native Methods

        internal const int WH_KEYBOARD_LL = 0x0D;

        internal const byte VK_CAPITAL = 0x14;
        internal const byte VK_CONTROL = 0x11;
        internal const byte VK_MENU = 0x12;
        internal const byte VK_SHIFT = 0x10;
        internal const byte VK_LSHIFT = 0xA0;
        internal const byte VK_RSHIFT = 0xA1;
        internal const byte VK_LALT = 0xA4;
        internal const byte VK_LCONTROL = 0xA2;
        internal const byte VK_NUMLOCK = 0x90;
        internal const byte VK_RALT = 0xA5;
        internal const byte VK_RCONTROL = 0xA3;
        internal const byte VK_RMENU = 0xA5;

        internal const int WM_KEYDOWN = 0x100;
        internal const int WM_SYSKEYDOWN = 0x104;
        internal const int WM_KEYUP = 0x101;
        internal const int WM_SYSKEYUP = 0x105;
        internal const int WM_DEADCHAR = 0x0103;
        internal const int WM_SYSDEADCHAR = 0x0107;

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public int key;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr extra;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(Key key);

        #endregion

        public event EventHandler<KeyEventArgs> KeyDown;
        public event EventHandler<KeyEventArgs> KeyUp;
        public event EventHandler<KeyEventArgs> KeyPress;

        private IntPtr ptrHook;
        private LowLevelKeyboardProc keyboardProcess;
        private KeyConverter _converter;

        public bool IsCapturing { get; private set; }

        /// <summary>
        /// Suppresses Windows from handling the keyboard input.
        /// </summary>
        public bool SuppressWindowsHandling { get; set; }

        /// <summary>
        /// Disables triggering repeated events for keys that are held.
        /// </summary>
        public bool DisableRepeat { get; set; }

        public KeyboardInterceptor()
        {
            _converter = new KeyConverter();
        }

        public void StartCapturing()
        {
            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                keyboardProcess = new LowLevelKeyboardProc(KeyboardHandler);
                ptrHook = SetWindowsHookEx(13, keyboardProcess, GetModuleHandle(module.ModuleName), 0);
            }
            IsCapturing = true;
        }

        private int _previousScancode;
        private int _previousFlags;

        private bool IsRepeat(KBDLLHOOKSTRUCT keyInfo)
        {
            var isRepeat = false;

            if(_previousScancode == keyInfo.scanCode && _previousFlags == keyInfo.flags)
            {
                isRepeat = true; ;
            }

            _previousScancode = keyInfo.scanCode;
            _previousFlags = keyInfo.flags;

            return isRepeat;
        }

        private IntPtr KeyboardHandler(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if(nCode >= 0)
            {
                KBDLLHOOKSTRUCT keyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

                var key = KeyInterop.KeyFromVirtualKey(keyInfo.key);

                if(DisableRepeat && IsRepeat(keyInfo))
                {
                    if(SuppressWindowsHandling)
                    {
                        return (IntPtr)1;
                    }
                    else
                    {
                        return CallNextHookEx(ptrHook, nCode, wParam, lParam);
                    }
                }
                else
                {
                    _previousScancode = keyInfo.scanCode;
                }

                var eventArgs = new KeyEventArgs(Keyboard.PrimaryDevice,
                                                 Keyboard.PrimaryDevice?.ActiveSource ?? new HwndSource(0, 0, 0, 0, 0, "", IntPtr.Zero),
                                                 keyInfo.time,
                                                 key);

                var intParam = wParam.ToInt32();
                switch(intParam)
                {
                    case WM_SYSKEYDOWN:
                        KeyDown?.Invoke(this, eventArgs);
                        break;
                    case WM_KEYDOWN:
                        KeyDown?.Invoke(this, eventArgs);
                        KeyPress?.Invoke(this, eventArgs);
                        break;
                    case WM_SYSKEYUP:
                    case WM_KEYUP:
                        KeyUp?.Invoke(this, eventArgs);
                        break;
                }
            }

            if(SuppressWindowsHandling)
            {
                return (IntPtr)1;
            }
            else
            {
                return CallNextHookEx(ptrHook, nCode, wParam, lParam);
            }
        }

        public void StopCapturing()
        {
            if(ptrHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(ptrHook);
                ptrHook = IntPtr.Zero;
            }
        }

    }
}
