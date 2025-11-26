using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FnafLike.Graphics;

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

        // detach and delete stuff, cleanup
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
