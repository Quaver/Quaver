/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Wobble.Window;

namespace Quaver.Shared.Window
{
    /// <summary>
    ///     Cross-platform multi-monitor lookups (Win32 on Windows, SDL2 on macOS/Linux), used to keep the
    ///     game window on the correct display across resolution changes and full screen toggles.
    /// </summary>
    public static class MonitorHelper
    {
        /// <summary>
        ///     Resolves "SDL2" to the same arch-subfolder DLL MonoGame initializes, not the uninitialized
        ///     copy .NET's default probing would otherwise find next to the executable.
        /// </summary>
        static MonitorHelper() =>
            NativeLibrary.SetDllImportResolver(typeof(MonitorHelper).Assembly, ResolveSdl2);

        private static IntPtr ResolveSdl2(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName != "SDL2")
                return IntPtr.Zero;

            var archFolder = Environment.Is64BitProcess ? "x64" : "x86";

            var path = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? Path.Combine(AppContext.BaseDirectory, "libSDL2-2.0.0.dylib")
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? Path.Combine(AppContext.BaseDirectory, archFolder, "libSDL2-2.0.so.0")
                    : Path.Combine(AppContext.BaseDirectory, archFolder, "SDL2.dll");

            return File.Exists(path) && NativeLibrary.TryLoad(path, out var handle) ? handle : IntPtr.Zero;
        }

        #region Win32 P/Invoke

        private const int MONITOR_DEFAULTTONEAREST = 2;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int Size;
            public RECT Monitor;
            public RECT WorkArea;
            public int Flags;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT pt, int flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        #endregion

        #region SDL2 P/Invoke

        [StructLayout(LayoutKind.Sequential)]
        private struct SDL_Rect { public int x, y, w, h; }

        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_GetNumVideoDisplays();

        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_GetDisplayBounds(int displayIndex, out SDL_Rect rect);

        private const uint SDL_WINDOW_MAXIMIZED = 0x00000080;

        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint SDL_GetWindowFlags(IntPtr window);

        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SDL_RestoreWindow(IntPtr window);

        #endregion

        /// <summary>
        ///     The bounds of a monitor, in desktop coordinates.
        /// </summary>
        public struct Bounds
        {
            public int Left, Top, Right, Bottom;
            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        /// <summary>
        ///     Finds the bounds of the monitor containing the given desktop point.
        ///     On Windows, uses Win32 <see cref="MonitorFromPoint"/>. On macOS/Linux, uses SDL2 display
        ///     bounds enumeration, since there is no Win32 user32.dll to call into there.
        /// </summary>
        public static Bounds? GetBoundsAtPoint(int x, int y) =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? GetBoundsAtPointWin32(x, y)
                : GetBoundsAtPointSdl(x, y);

        private static Bounds? GetBoundsAtPointWin32(int x, int y)
        {
            var monitor = MonitorFromPoint(new POINT { X = x, Y = y }, MONITOR_DEFAULTTONEAREST);
            var info = new MONITORINFO { Size = Marshal.SizeOf<MONITORINFO>() };

            if (!GetMonitorInfo(monitor, ref info))
                return null;

            return new Bounds
            {
                Left = info.Monitor.Left,
                Top = info.Monitor.Top,
                Right = info.Monitor.Right,
                Bottom = info.Monitor.Bottom
            };
        }

        private static Bounds? GetBoundsAtPointSdl(int x, int y)
        {
            try
            {
                var displayCount = SDL_GetNumVideoDisplays();

                for (var i = 0; i < displayCount; i++)
                {
                    if (SDL_GetDisplayBounds(i, out var bounds) != 0)
                        continue;

                    if (x < bounds.x || x >= bounds.x + bounds.w ||
                        y < bounds.y || y >= bounds.y + bounds.h)
                        continue;

                    return new Bounds
                    {
                        Left = bounds.x,
                        Top = bounds.y,
                        Right = bounds.x + bounds.w,
                        Bottom = bounds.y + bounds.h
                    };
                }
            }
            catch (DllNotFoundException)
            {
                // SDL2 isn't guaranteed to be loadable under every runtime configuration.
            }

            return null;
        }

        /// <summary>
        ///     Restores <paramref name="window"/> if the OS has it maximized - a maximized window's size
        ///     is pinned by the OS, so resize requests get silently clamped until it's cleared. Goes
        ///     through SDL's own maximized state rather than a platform-specific API, so it works the
        ///     same on Windows, macOS, and Linux.
        /// </summary>
        /// <returns>Whether the window was maximized and got restored.</returns>
        public static bool RestoreIfMaximized(GameWindow window)
        {
            if ((SDL_GetWindowFlags(window.Handle) & SDL_WINDOW_MAXIMIZED) == 0)
                return false;

            SDL_RestoreWindow(window.Handle);
            return true;
        }

        /// <summary>
        ///     Centers <paramref name="window"/> within the given monitor bounds.
        /// </summary>
        public static void CenterOnMonitor(GameWindow window, Bounds bounds, int targetWidth, int targetHeight)
        {
            var x = bounds.Left + (bounds.Width - targetWidth) / 2;
            var y = bounds.Top + (bounds.Height - targetHeight) / 2;
            window.Position = new Point(x, y);
        }

        /// <summary>
        ///     Centers <paramref name="window"/> on the monitor it was previously on (derived from its
        ///     old position and size), falling back to a plain arithmetic center if the monitor can no
        ///     longer be resolved.
        /// </summary>
        public static void CenterOnCurrentMonitor(GameWindow window, Point oldPos, int oldWidth, int oldHeight,
            int targetWidth, int targetHeight)
        {
            var centerX = oldPos.X + oldWidth / 2;
            var centerY = oldPos.Y + oldHeight / 2;

            var bounds = GetBoundsAtPoint(centerX, centerY);

            if (bounds.HasValue)
            {
                CenterOnMonitor(window, bounds.Value, targetWidth, targetHeight);
                return;
            }

            window.Position = new Point(centerX - targetWidth / 2, centerY - targetHeight / 2);
        }
    }
}
