using Microsoft.UI;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Graphics;
using WsiuEngine.Core.Interfaces;
using WsiuEngine.Extensions;
using WsiuRenderer;

namespace WsiuEngine.Core.System
{
    public partial class Screen
    {
        private static Screen instance = null!;
        internal static void Initialize(nint hwnd)
        {
            instance = new Screen(hwnd);
        }

        private Screen(nint hwnd)
        {
            _hwnd = hwnd;
            _windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            _appWindow = AppWindow.GetFromWindowId(_windowId);
        }

        private readonly nint _hwnd;
        private readonly WindowId _windowId;
        private readonly AppWindow _appWindow;

        public struct Bounds(int x, int y, int width, int height) : IReflectionDrawer
        {
            public int X = x;
            public int Y = y;
            public int Width = width;
            public int Height = height;

            readonly bool IReflectionDrawer.UseCustomDrawing => true;
            readonly void IReflectionDrawer.DrawFields(ImguiContext context, string name, bool isReadOnly, IReadOnlyDictionary<Type, Attribute>? attributes)
            {
                Type type = typeof(int);
                context.TreeNodeEx(name, ImGuiTreeNodeFlags.None);
                context.PushStyleReadOnly();
                context.BeginTablePropertyType(GetHashCode().ToString());
                DrawField(context, type, X, nameof(X));
                DrawField(context, type, Y, nameof(Y));
                DrawField(context, type, Width, nameof(Width));
                DrawField(context, type, Height, nameof(Height));
                context.EndTable();
                context.PopStyleReadOnly();
                context.TreePop();
            }

            private static void DrawField(ImguiContext context, Type type, int field, string name)
            {
                context.TableNextRow();
                context.TableNextColumn();
                context.TextUnformatted(name);
                context.TableNextColumn();
                ReflectionObject.DrawField(context, type, name, field, true, (v) => { });
            }
        }

        public static int Width => instance.InternalGetAppWindowSize().Width;
        public static int Height => instance.InternalGetAppWindowSize().Height;
        internal SizeInt32 InternalGetAppWindowSize() => _appWindow.Size;

        public static int PosX => instance.InternalGetAppWindowPosition().X;
        public static int PosY => instance.InternalGetAppWindowPosition().Y;

        internal PointInt32 InternalGetAppWindowPosition() => _appWindow.Position;

        public static bool IsMaximized => instance.IsMaximizedWindow();
        public bool IsMaximizedWindow() => _appWindow.Presenter is OverlappedPresenter { State: OverlappedPresenterState.Maximized };

        public static bool IsFullScreen => instance.IsFullScreenWindow();
        public bool IsFullScreenWindow() => _appWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen;


        public static void Move(int x, int y) => instance.InternalMove(x, y);
        internal void InternalMove(int x, int y)
        {
            _appWindow.Move(new(x, y));
        }

        public static void Resize(int width, int height) => instance.InternalResize(width, height);
        internal void InternalResize(int width, int height)
        {
            _appWindow.ResizeClient(new(width, height));
        }


        public static void Maximize() => instance.InternalMaximize();
        internal void InternalMaximize()
        {
            if (_appWindow.Presenter is OverlappedPresenter overlapped)
                overlapped.Maximize();
        }

        [HideInInspector]
        public static Bounds RestoreBounds => instance.InternalGetRestoreBounds();
        internal Bounds InternalGetRestoreBounds()
        {
            WINDOWPLACEMENT wp = new()
            {
                length = Marshal.SizeOf<WINDOWPLACEMENT>()
            };
            if (GetWindowPlacement(_hwnd, ref wp))
            {
                int x = wp.rcNormalPosition.left;
                int y = wp.rcNormalPosition.top;
                int width = wp.rcNormalPosition.right - wp.rcNormalPosition.left;
                int height = wp.rcNormalPosition.bottom - wp.rcNormalPosition.top;
                return new(x, y, width, height);
            }
            return new(PosX, PosY, Width, Height);
        }
    }
}
