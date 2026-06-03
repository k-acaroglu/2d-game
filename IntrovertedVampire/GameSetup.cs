using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Zenseless.OpenTK;
using System.Diagnostics;

public static class GameSetup
{
    public static GameWindow Create(string title)
    {
        // center the window on the primary monitor with half the resolution
        var info = Monitors.GetPrimaryMonitor();
        Vector2i res = new(info.HorizontalResolution, info.VerticalResolution);
        Vector2i winRes = res / 2;
        Vector2i pos = (res - winRes) / 2;
        GameWindow window = new(GameWindowSettings.Default, ImmediateMode.NativeWindowSettings)
        {
            ClientRectangle = new Box2i(pos, pos + winRes),
            VSync = VSyncMode.On,
            Title = title
        };

        // set up event handlers
        window.Resize += args => GL.Viewport(0, 0, args.Width, args.Height);
        window.KeyDown += args => { if (args.Key == Keys.Escape) window.Close(); };

        // start the stopwatch for frame timing
        window.ResetTimeSinceLastUpdate();

        // set the initial viewport
        var s = window.ClientSize;
        GL.Viewport(0, 0, s.X, s.Y);
        return window;
    }

    // advances to the next frame, updates the time delta, and processes window events
    public static bool NextFrame(this GameWindow window)
    {
        // calculate the time delta since the last frame
        TimeDelta = (float)Stopwatch.Elapsed.TotalSeconds;
        Stopwatch.Restart();
        window.SwapBuffers();
        window.NewInputFrame();
        NativeWindow.ProcessWindowEvents(false);
        return !window.IsExiting;
    }

    // measures how long the last frame took and updates the TimeDelta  accordingly
    public static float TimeDelta { get; private set; } = 1f / 60f;
    private static readonly Stopwatch Stopwatch = Stopwatch.StartNew();
}