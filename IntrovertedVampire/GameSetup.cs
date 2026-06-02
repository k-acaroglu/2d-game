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

        window.Resize += args => GL.Viewport(0, 0, args.Width, args.Height);
        window.KeyDown += args => { if (args.Key == Keys.Escape) window.Close(); };
        window.ResetTimeSinceLastUpdate();

        var s = window.ClientSize;
        GL.Viewport(0, 0, s.X, s.Y);
        return window;
    }

    public static bool NextFrame(this GameWindow window)
    {
        TimeDelta = (float)Stopwatch.Elapsed.TotalSeconds;
        Stopwatch.Restart();
        window.SwapBuffers();
        window.NewInputFrame();
        NativeWindow.ProcessWindowEvents(false);
        return !window.IsExiting;
    }

    public static float TimeDelta { get; private set; } = 1f / 60f;

    private static readonly Stopwatch Stopwatch = Stopwatch.StartNew();
}