using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using FnafLike.Game;
using FnafLike.Graphics;

namespace FnafLike.Core;

#region Game Code

class Game : GameWindow
{
    private RectRenderer? _renderer;
    private Matrix4 _projPixelsToNdc;

    private Door? _leftDoor;
    private Door? _rightDoor;

    public Game(NativeWindowSettings settings)
        : base(GameWindowSettings.Default, settings)
    { }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.08f, 0.08f, 0.1f, 1f);
        GL.Viewport(0, 0, Size.X, Size.Y); // The viewport function covers the new pixel area on resizing the window

        _renderer = new RectRenderer();
        UpdateProjection();
        // we have to update projection before drawing because the uMVP matrix must be updated with correct projection

        // Lay out doors as big vertical panels on left/right with a small margin.
        float margin = 20f;
        float half = Size.X / 2f;

        _leftDoor = new Door(
            "left",
            new Rect(margin, margin, half - margin * 1.5f, Size.Y),
            new Vector3(0.20f, 0.35f, 0.80f) // blue
        );

        _rightDoor = new Door(
            "right",
            new Rect(half + margin * 0.5f, margin, half - margin * 1.5f, Size.Y - margin * 2f),
            new Vector3(0.80f, 0.35f, 0.20f) // orange
        );
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, Size.X, Size.Y);
        UpdateProjection();

        // Recompute panel sizes to keep margins consistent on resize.
        float margin = 20f;
        float half = Size.X / 2f;

        if (_leftDoor != null)
            _leftDoor.Bounds = new Rect(margin, margin, half - margin * 1.5f, Size.Y - margin * 2f);
        if (_rightDoor != null)
            _rightDoor.Bounds = new Rect(half + margin * 0.5f, margin, half - margin * 1.5f, Size.Y - margin * 2f);
    }

    private void UpdateProjection()
    {
        // Orthographic projection mapping pixels → NDC, with (0,0) at top-left.
        _projPixelsToNdc = Matrix4.CreateOrthographicOffCenter
        (
            0     , Size.X,
            Size.Y, 0     ,   // invert Y to make +Y downward
            -1    , 1
        );
        // left, right, bottom, top, near, far
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        if (_renderer != null && _leftDoor != null && _rightDoor != null)
        {
            _renderer.Draw(_leftDoor.Bounds, _leftDoor.Color, _projPixelsToNdc);
            _renderer.Draw(_rightDoor.Bounds, _rightDoor.Color, _projPixelsToNdc);
        }

        SwapBuffers();
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        if (_leftDoor == null || _rightDoor == null) return;

        var p = new Vector2(MousePosition.X, MousePosition.Y); // top-left origin

        if (_leftDoor.Bounds.Contains(p))
            Console.WriteLine("The left door was clicked");
        else if (_rightDoor.Bounds.Contains(p))
            Console.WriteLine("The right door was clicked");
        else
            Console.WriteLine("Clicked background");
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();
    }

    protected override void OnUnload()
    {
        _renderer?.Dispose();
        base.OnUnload();
    }
}

#endregion
