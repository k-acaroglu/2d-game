using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

#region Structs/classes

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

#region Rectangle renderer

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

        // generate vao
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        // generate vbo
        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);


        GL.EnableVertexAttribArray(0); // tells opengl where vertices are (turn on vertex input at position 0)
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0); // tells opengl to actually use them

        // shader stuff
        _shader = new Shader(VertexSrc, FragmentSrc);

        // unbind
        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public void Draw(Rect rect, Vector3 rgb, Matrix4 proj)
    {
        _shader.Use();

        // model = translate(x,y) * scale(w,h) -> converts local vertex positions into pixel coordinates for rect
        var model =  Matrix4.CreateScale(rect.W, rect.H, 1f) * Matrix4.CreateTranslation(rect.X, rect.Y, 0f);

        // combines model and projection into single matrix for ndc, cuz it's what gpu expects
        var mvp = model * proj; // proj is orthographic (pixels → NDC)

        // THIS IS ALL THEORY SO JUST WRITING IT HERE AS REMINDER CUZ FUCK LOW LEVEL SHIT
        // the 'travel route' of a vertex is: local -> world -> view -> clip -> ndc -> screen
        // the vertices created are in local space, which are converted into world space with model matrix
        // because this is a 2D proejct, view space is skipped. clip space happens with uMVP, now we have -w and +w
        // after clipping, gpu divides everything by w (x' = x/w, y' = x/w and so on) to make all coodinates between -1 and +1
        // this aligns us with the opengl coords we're comfy with (-1, -1 is bottom left, -1 z is near and such)
        // clip to ndc happens in the function below (UpdateProjection())
        // finally, gpu converts ndc into pixel positions with viewport size (0,0 is bottom left; width,height s top right etc.)

        // sets the uniforms
        _shader.Set("uMVP", mvp); // uploads mvp matrix and color to shader uniform named umvp
        _shader.Set("uColor", rgb); // uColor is output color

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

    // GLSL type shit
    // these two are shader objects, we can compile them with input/outputs
    // then we can link them and get a "program object". this determines how we handle vertices
    // uniforms are set from the CPU side (C#) and stay the same for all vertices/pixels during one draw call
    private const string VertexSrc =
        "#version 330 core\n" + // version

        // basically transform rectangle/s local coordinates into screen coordinates
        "layout (location = 0) in vec2 aPos;\n" + // input vertex positions from vao/vbo
        "uniform mat4 uMVP;\n" + // "camera matrix"

        "void main(){\n" +
        "    gl_Position = uMVP * vec4(aPos, 0.0, 1.0);\n" + // called for per vertex, converts vertices into clip-space position
        "}\n";

    private const string FragmentSrc =
        "#version 330 core\n" +

        "out vec4 FragColor;\n" + // color of each pixel
        "uniform vec3 uColor;\n" + // rgb color sent from cpu

        "void main(){\n" +
        "    FragColor = vec4(uColor, 1.0);\n" + // called for per pixel, combines and outputs final color value
        "}\n";
}

#endregion

#region Shader helper

class Shader : IDisposable
{
    public int compiledProgramID { get; }

    public Shader(string vertexSource, string fragmentSource)
    {
        // compile sources
        int vertexShader = Compile(ShaderType.VertexShader, vertexSource);
        int fragmentShader = Compile(ShaderType.FragmentShader, fragmentSource);

        compiledProgramID = GL.CreateProgram(); // allocates a program object and returns int handle

        // attach the compiled shaders into the handle, then link it
        GL.AttachShader(compiledProgramID, vertexShader);
        GL.AttachShader(compiledProgramID, fragmentShader);
        GL.LinkProgram(compiledProgramID);
        GL.GetProgram(compiledProgramID, GetProgramParameterName.LinkStatus, out int ok);
        if (ok == 0)
        {
            string info = GL.GetProgramInfoLog(compiledProgramID);
            throw new Exception($"Program link error:\n{info}");
        }

        // detach and delete stuff
        GL.DetachShader(compiledProgramID, vertexShader);
        GL.DetachShader(compiledProgramID, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    private static int Compile(ShaderType type, string src)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, src);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int ok);
        if (ok == 0)
        {
            string info = GL.GetShaderInfoLog(shader);
            throw new Exception($"{type} compile error:\n{info}");
        }
        return shader;
    }

    public void Use() => GL.UseProgram(compiledProgramID);

    // these set functions upload uniform values from C# into the GPU shader uniforms
    public void Set(string name, Matrix4 value) // matrix version
    {
        int location = GL.GetUniformLocation(compiledProgramID, name);
        GL.UniformMatrix4(location, false, ref value); // this says "replace uMVP in the shader with this matrix"
    }

    public void Set(string name, Vector3 value) // vector3 version
    {
        int location = GL.GetUniformLocation(compiledProgramID, name);
        GL.Uniform3(location, value); // this says "replace uColor in the shader with this rgb value"
    }

    public void Dispose() => GL.DeleteProgram(compiledProgramID);
}

#endregion

#region Game Code

class Game : GameWindow
{
    private RectRenderer? _renderer;
    private Matrix4 _projPixelsToNdc;

    private Door? _leftDoor;
    private Door? _rightDoor;

    public Game()
        : base(GameWindowSettings.Default, new NativeWindowSettings
        {
            Title = "Introverted Vampire",
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

class Program
{
    static void Main()
    {
        using var game = new Game();
        game.Run();
    }
}

#endregion