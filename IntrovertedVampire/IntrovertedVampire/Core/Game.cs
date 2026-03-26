using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using IntrovertedVampire.Game;
using IntrovertedVampire.Graphics;

namespace IntrovertedVampire.Core;

class Game : GameWindow
{

    // renderer for drawing rectangles
    private RectRenderer? _renderer;
    private Matrix4 _projPixelsToNdc;


    // game elements
    private Door? _leftDoor;
    private Door? _rightDoor;
    private float _lookX = 0f; // -1 (left) to +1 (right) and 0 is center


    // fade thresholds
    private const float FadeStartLeft = -0.6f;
    private const float FadeFullLeft = -0.9f;
    private const float FadeStartRight = +0.6f;
    private const float FadeFullRight = +0.9f;

    // view thresholds
    private const float LeftEdgeThreshold = -0.9f;
    private const float RightEdgeThreshold = +0.9f;
    private const float CenterAreaAbs = 0.2f;



    // view-band helpers
    private bool InLeftEdge() => _lookX <= LeftEdgeThreshold;
    private bool InRightEdge() => _lookX >= RightEdgeThreshold;
    private bool InCenterBand() => MathF.Abs(_lookX) < CenterAreaAbs;
    private static float InverseLerp(float a, float b, float x) // lerp = linear interpolation
    {
        // this returns how far x is between a and b as dist ∈ [0..1]
        if (Math.Abs(b - a) < 1e-6f) return 0f; // avoid div by zero
        var dist = (x - a) / (b - a); // compute distance
        if (dist < 0f) return 0f;
        if (dist > 1f) return 1f;
        return dist;
    }


    // background settings
    private Rect _background;
    private Vector3 _bgColor = new Vector3(0.2f, 0.2f, 0.08f);
    private const float _bgParallaxPx = 60f; // how much the background extends beyond the window edges

    // center monitor area
    private Rect _centerMonitor;
    private Vector3 _centerMonitorColor = new Vector3(0.1f, 0.75f, 0.75f);

    // We only print band when it changes
    private enum ViewBand { Left, Center, Right }
    private ViewBand _currentView = ViewBand.Center;

    // door colors (rgb kept here; alpha computed per frame)
    private Vector3 _leftDoorColor = new Vector3(0.20f, 0.35f, 0.80f); // blue
    private Vector3 _rightDoorColor = new Vector3(0.80f, 0.35f, 0.20f); // orange

    // edge-door sizing
    private const float DoorWidthPx = 100f;

    // rng + timing config
    private readonly Random _rng = new();
    private const float OpenDurationSec = 4.0f;  // from start of opening → fully open (loss)
    private const float OpenMinDelaySec = 3.0f;  // random closed delay range
    private const float OpenMaxDelaySec = 7.0f;

    // left door runtime state
    private bool _leftIsOpening = false;
    private float _leftOpenProgress = 0f;        // 0..OpenDurationSec while opening
    private float _leftNextOpenTimer = 0f;       // counts down while closed

    // right door runtime state
    private bool _rightIsOpening = false;
    private float _rightOpenProgress = 0f;       // 0..OpenDurationSec while opening
    private float _rightNextOpenTimer = 0f;      // counts down while closed

    // simple game state
    private bool _gameLost = false;

    public Game(NativeWindowSettings settings) : base(GameWindowSettings.Default, settings)
    { }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.08f, 0.08f, 0.1f, 1f);
        GL.Viewport(0, 0, Size.X, Size.Y); // The viewport function covers the new pixel area on resizing the window

        // enable alpha blending for transparency of doors
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        // final color = (srcColor * srcFactor) + (dstColor * dstFactor)

        _renderer = new RectRenderer();
        UpdateProjection();
        // we have to update projection before drawing because the uMVP matrix must be updated with correct projection

        // Lay out doors as big vertical panels on left/right with a small margin.
        LayoutDoorsAndUI();

        ScheduleNextOpenLeft();
        ScheduleNextOpenRight();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, Size.X, Size.Y);
        UpdateProjection();
        LayoutDoorsAndUI();
    }

    private void LayoutDoorsAndUI()
    {
        {
            // Doors now match left/right edges as thin panels
            _leftDoor = new Door("left", new Rect(0f, 0f, DoorWidthPx, Size.Y), _leftDoorColor);
            _rightDoor = new Door("right", new Rect(Size.X - DoorWidthPx, 0f, DoorWidthPx, Size.Y), _rightDoorColor);

            // Background spans the full height and a bit wider for parallax
            _background = new Rect(-_bgParallaxPx, 0f, Size.X + 2f * _bgParallaxPx, Size.Y);

            // center monitor
            var monitorW = 360f;
            var monitorH = 220f;
            _centerMonitor = new Rect(
                (Size.X - monitorW) * 0.5f,
                (Size.Y - monitorH) * 0.5f,
                monitorW,
                monitorH
            );
        }
    }

    private void UpdateProjection()
    {
        // Orthographic projection mapping pixels → NDC, with (0,0) at top-left.
        _projPixelsToNdc = Matrix4.CreateOrthographicOffCenter
        (
            0, Size.X,
            Size.Y, 0,   // invert Y to make +Y downward
            -1, 1
        );
        // left, right, bottom, top, near, far
    }

    private float RandomDelay() =>
        (float)(_rng.NextDouble() * (OpenMaxDelaySec - OpenMinDelaySec) + OpenMinDelaySec);

    private void ScheduleNextOpenLeft()
    {
        _leftIsOpening = false;
        _leftOpenProgress = 0f;
        _leftNextOpenTimer = RandomDelay();
    }

    private void ScheduleNextOpenRight()
    {
        _rightIsOpening = false;
        _rightOpenProgress = 0f;
        _rightNextOpenTimer = RandomDelay();
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        if (_leftDoor == null || _rightDoor == null) return;

        // Mouse in pixel coords (top-left origin)
        var p = new Vector2(MousePosition.X, MousePosition.Y);

        // Doors: must be at the corresponding edge AND inside the door rect
        if (InLeftEdge() && _leftDoor.Bounds.Contains(p))
        {
            // NEW: if opening, clicking closes instantly and reschedules
            if (_leftIsOpening)
            {
                Console.WriteLine("left door closed");
                ScheduleNextOpenLeft();
            }
            else
            {
                Console.WriteLine("left door is already closed");
            }
            return;
        }

        if (InRightEdge() && _rightDoor.Bounds.Contains(p))
        {
            if (_rightIsOpening)
            {
                Console.WriteLine("right door closed");
                ScheduleNextOpenRight();
            }
            else
            {
                Console.WriteLine("right door is already closed");
            }
            return;
        }

        // Center monitor: ALWAYS accessible (rect only)
        if (_centerMonitor.Contains(p))
        {
            Console.WriteLine("center monitor is being interacted");
            return;
        }

        Console.WriteLine("clicked background");
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        // simple exit shortcut
        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();

        if (_gameLost)
            return; // freezes game logic

        float dt = (float)args.Time;

        // mouse look is clamped between -1 and +1
        float mouseLook = 0f;
        if (Size.X > 0)
        {
            mouseLook = Math.Clamp((MousePosition.X / Math.Max(1f, Size.X)) * 2f - 1f, -1f, 1f);
        }

        // Keyboard overrides for testing
        if (KeyboardState.IsKeyDown(Keys.A) || KeyboardState.IsKeyDown(Keys.Left))
            _lookX = -1f;
        else if (KeyboardState.IsKeyDown(Keys.S))
            _lookX = 0f;
        else if (KeyboardState.IsKeyDown(Keys.D) || KeyboardState.IsKeyDown(Keys.Right))
            _lookX = +1f;
        else
            _lookX = mouseLook; // default: follow mouse

        // Determine which band we are in (for debug prints)
        var newBand =
            (_lookX <= LeftEdgeThreshold) ? ViewBand.Left :
            (_lookX >= RightEdgeThreshold) ? ViewBand.Right :
            (MathF.Abs(_lookX) < CenterAreaAbs ? ViewBand.Center :
             (_currentView == ViewBand.Center ? ViewBand.Center : _currentView)); // remain if in the transitional zone

        if (newBand != _currentView)
        {
            _currentView = newBand;
        }

        if (_leftIsOpening)
        {
            _leftOpenProgress += dt;
            if (_leftOpenProgress >= OpenDurationSec)
            {
                Console.WriteLine("left door fully opened! Game Lost!");
                _gameLost = true;
            }
        }
        else
        {
            _leftNextOpenTimer -= dt;
            if (_leftNextOpenTimer <= 0f)
            {
                _leftIsOpening = true;
                _leftOpenProgress = 0f;
                Console.WriteLine("left door is opening!");
            }
        }

        // Right door
        if (_rightIsOpening)
        {
            _rightOpenProgress += dt;
            if (_rightOpenProgress >= OpenDurationSec)
            {
                Console.WriteLine("right door fully opened! Game Lost!");
                _gameLost = true;
            }
        }
        else
        {
            _rightNextOpenTimer -= dt;
            if (_rightNextOpenTimer <= 0f)
            {
                _rightIsOpening = true;
                _rightOpenProgress = 0f;
                Console.WriteLine("right door is opening!");
            }
        }
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        if (_renderer != null)
        {
            // parallax: slide the background horizontally by lookX * _bgParallaxPx
            var bg = _background;
            bg.X = -_bgParallaxPx + _lookX * _bgParallaxPx;
            _renderer.Draw(bg, new Vector4(_bgColor, 1f), _projPixelsToNdc);

            // center monitor (always visible)
            _renderer.Draw(_centerMonitor, new Vector4(_centerMonitorColor, 1f), _projPixelsToNdc);
        }

        // door transparency based on lookX
        if (_renderer != null && _leftDoor != null && _rightDoor != null)
        {
            // compute per-door alpha based on lookX
            float leftAlpha = 0f;
            if (_lookX <= FadeStartLeft)
            {
                leftAlpha = InverseLerp(FadeStartLeft, FadeFullLeft, _lookX);
            }

            float rightAlpha = 0f;
            if (_lookX >= FadeStartRight)
            {
                rightAlpha = InverseLerp(FadeStartRight, FadeFullRight, _lookX);
            }

            _renderer.Draw(_leftDoor.Bounds, new Vector4(_leftDoor.Color, leftAlpha), _projPixelsToNdc);
            _renderer.Draw(_rightDoor.Bounds, new Vector4(_rightDoor.Color, rightAlpha), _projPixelsToNdc);
        }

        SwapBuffers();
    }

    protected override void OnUnload()
    {
        _renderer?.Dispose();
        base.OnUnload();
    }
}