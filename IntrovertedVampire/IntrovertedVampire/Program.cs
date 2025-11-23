using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

#region Small domain types

// Pixel-space rectangle with top-left origin (0,0) at the window's top-left.
struct Rect
{
    public float X, Y, W, H;
    public Rect(float x, float y, float w, float h) { X = x; Y = y; W = w; H = h; }

    public bool Contains(Vector2 p)
        => p.X >= X && p.X <= X + W && p.Y >= Y && p.Y <= Y + H;
}

class Door
{
    public string Name { get; }
    public Rect Bounds;
    public Vector3 Color;

    public Door(string name, Rect bounds, Vector3 color)
    {
        Name = name;
        Bounds = bounds;
        Color = color;
    }
}

#endregion

#region Simple 2D rectangle renderer (unit quad + MVP)

class RectRenderer : IDisposable
{
    private readonly int _vao, _vbo;
    private readonly Shader _shader;

    public RectRenderer()
    {
        // Unit quad in local space drawn with triangles
        float[] verts =
        [
            // x, y
            0f, 0f,
            1f, 0f,
            1f, 1f,

            0f, 0f,
            1f, 1f,
            0f, 1f,
        ];

        // generate vao and vbo
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

        _shader = new Shader(VertexSrc, FragmentSrc);

        // unbind
        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public void Draw(Rect rect, Vector3 rgb, Matrix4 proj)
    {
        _shader.Use();

        // Model = translate(x,y) * scale(w,h)
        var model =
            Matrix4.CreateScale(rect.W, rect.H, 1f) *
            Matrix4.CreateTranslation(rect.X, rect.Y, 0f);

        var mvp = model * proj; // proj is orthographic (pixels → NDC)

        _shader.Set("uMVP", mvp);
        _shader.Set("uColor", rgb);

        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_vao);
        _shader?.Dispose();
    }

    // Using explicit \n to avoid any accidental formatting issues
    private const string VertexSrc =
        "#version 330 core\n" +
        "layout (location = 0) in vec2 aPos;\n" +
        "uniform mat4 uMVP;\n" +
        "void main(){\n" +
        "    gl_Position = uMVP * vec4(aPos, 0.0, 1.0);\n" +
        "}\n";

    private const string FragmentSrc =
        "#version 330 core\n" +
        "out vec4 FragColor;\n" +
        "uniform vec3 uColor;\n" +
        "void main(){\n" +
        "    FragColor = vec4(uColor, 1.0);\n" +
        "}\n";
}

#endregion

#region Tiny shader helper

class Shader : IDisposable
{
    public int Handle { get; }

    public Shader(string vertexSource, string fragmentSource)
    {
        int vs = Compile(ShaderType.VertexShader, vertexSource);
        int fs = Compile(ShaderType.FragmentShader, fragmentSource);

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vs);
        GL.AttachShader(Handle, fs);
        GL.LinkProgram(Handle);
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int ok);
        if (ok == 0)
        {
            string info = GL.GetProgramInfoLog(Handle);
            throw new Exception($"Program link error:\n{info}");
        }
        GL.DetachShader(Handle, vs);
        GL.DetachShader(Handle, fs);
        GL.DeleteShader(vs);
        GL.DeleteShader(fs);
    }

    private static int Compile(ShaderType type, string src)
    {
        int sh = GL.CreateShader(type);
        GL.ShaderSource(sh, src);
        GL.CompileShader(sh);
        GL.GetShader(sh, ShaderParameter.CompileStatus, out int ok);
        if (ok == 0)
        {
            string info = GL.GetShaderInfoLog(sh);
            throw new Exception($"{type} compile error:\n{info}");
        }
        return sh;
    }

    public void Use() => GL.UseProgram(Handle);

    public void Set(string name, Matrix4 value)
    {
        int loc = GL.GetUniformLocation(Handle, name);
        GL.UniformMatrix4(loc, false, ref value);
    }

    public void Set(string name, Vector3 value)
    {
        int loc = GL.GetUniformLocation(Handle, name);
        GL.Uniform3(loc, value);
    }

    public void Dispose() => GL.DeleteProgram(Handle);
}

#endregion

class Game : GameWindow
{
    private RectRenderer? _renderer;
    private Matrix4 _projPixelsToNdc;

    private Door? _leftDoor;
    private Door? _rightDoor;

    public Game()
        : base(GameWindowSettings.Default, new NativeWindowSettings
        {
            Title = "FNAF-like Prototype",
            ClientSize = new Vector2i(1280, 720),
            Flags = ContextFlags.ForwardCompatible,
        })
    { }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.08f, 0.08f, 0.1f, 1f);
        GL.Viewport(0, 0, Size.X, Size.Y); // The viewport function covers the new pixel area on resizing the window

        _renderer = new RectRenderer();
        UpdateProjection();

        // Lay out doors as big vertical panels on left/right with a small margin.
        float margin = 20f;
        float half = Size.X / 2f;

        _leftDoor = new Door(
            "LEFT",
            new Rect(margin, margin, half - margin * 1.5f, Size.Y - margin * 2f),
            new Vector3(0.20f, 0.35f, 0.80f) // bluish
        );

        _rightDoor = new Door(
            "RIGHT",
            new Rect(half + margin * 0.5f, margin, half - margin * 1.5f, Size.Y - margin * 2f),
            new Vector3(0.80f, 0.35f, 0.20f) // reddish
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
        _projPixelsToNdc = Matrix4.CreateOrthographicOffCenter(
            0, Size.X,
            Size.Y, 0,   // invert Y to make +Y downward
            -1, 1);
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
            Console.WriteLine("The LEFT door was clicked");
        else if (_rightDoor.Bounds.Contains(p))
            Console.WriteLine("The RIGHT door was clicked");
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

class Program
{
    static void Main()
    {
        using var game = new Game();
        game.Run();
    }
}
