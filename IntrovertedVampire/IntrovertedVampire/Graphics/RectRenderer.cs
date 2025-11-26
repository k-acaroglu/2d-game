using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using FnafLike.Game;

namespace FnafLike.Graphics;

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
        var model =  Matrix4.CreateScale(rect.RectangleWidth, rect.RectangleHeight, 1f) * Matrix4.CreateTranslation(rect.X, rect.Y, 0f);

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
