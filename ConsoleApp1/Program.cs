// See https://aka.ms/new-console-template for more information
//See https://opentk.net/learn/chapter1/1-getting-started.html
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

Console.WriteLine("Hello, World!");

var settings = new NativeWindowSettings()
{
    Profile = ContextProfile.Compatability,
    Flags = ContextFlags.Default,
};

float i = -1f; // This must be declared before Window_RenderFrame so that it can be used there
GameWindow window = new GameWindow(GameWindowSettings.Default, settings);
window.VSync = VSyncMode.On;
window.RenderFrame += Window_RenderFrame;
window.Run(); // Game loop
void Window_RenderFrame(OpenTK.Windowing.Common.FrameEventArgs obj)
{
    if (window.IsKeyDown(Keys.Left))
    {
        i -= 0.01f;
    }
    if (window.IsKeyDown(Keys.Right))
    {
        i += 0.01f;
    }
    if (i > 1f) i = -1f;
    if (i < -1f) i = 1f;
    GL.Viewport(0, 0, window.ClientSize.X, window.ClientSize.Y);
    GL.ClearColor(0.7f, 0.3f, 0.3f, 1.0f);
    GL.Clear(ClearBufferMask.ColorBufferBit);
    DrawTriangle(i);

    window.SwapBuffers();
    Console.WriteLine("Rendering a frame");
}

Console.WriteLine("After the window run");

static void DrawTriangle(float i)
{
    GL.Begin(BeginMode.Triangles);
    GL.Color3(1f, 0f, 0f);
    GL.Vertex2(i + 0f, 0f);
    GL.Color3(0f, 1f, 0f);
    GL.Vertex2(i + 1f, 0f);
    GL.Color3(0f, 0f, 1f);
    GL.Vertex2(i + 0f, 1f);
    GL.End();
}

// using OpenTK.Graphics.OpenGL4;
// using OpenTK.Mathematics;
// using OpenTK.Windowing.Common;
// using OpenTK.Windowing.Desktop;
// using OpenTK.Windowing.GraphicsLibraryFramework;

// // Simple OpenTK Game Window
// public class Game : GameWindow
// {
//     public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
//         : base(gameWindowSettings, nativeWindowSettings)
//     {
//     }

//     protected override void OnLoad()
//     {
//         base.OnLoad();

//         // Set the clear color (background color) to a nice blue
//         GL.ClearColor(0.2f, 0.3f, 0.8f, 1.0f);

//         Console.WriteLine("OpenTK window loaded successfully!");
//     }

//     protected override void OnRenderFrame(FrameEventArgs e)
//     {
//         base.OnRenderFrame(e);

//         // Clear the screen
//         GL.Clear(ClearBufferMask.ColorBufferBit);

//         // Swap the front/back buffers so it displays what we just rendered
//         SwapBuffers();
//     }

//     protected override void OnUpdateFrame(FrameEventArgs e)
//     {
//         base.OnUpdateFrame(e);

//         // Check if the Escape key is pressed
//         if (KeyboardState.IsKeyDown(Keys.Escape))
//         {
//             Close();
//         }
//     }

//     protected override void OnResize(ResizeEventArgs e)
//     {
//         base.OnResize(e);

//         // Update the viewport when the window is resized
//         GL.Viewport(0, 0, Size.X, Size.Y);
//     }
// }

// // Program entry point
// class Program
// {
//     public static void Main()
//     {
//         var gameWindowSettings = GameWindowSettings.Default;
//         var nativeWindowSettings = new NativeWindowSettings()
//         {
//             ClientSize = new Vector2i(800, 600),
//             Title = "Simple OpenTK Window",
//             // Set the window to be resizable
//             WindowBorder = WindowBorder.Resizable,
//             StartVisible = false,
//         };

//         using (var game = new Game(gameWindowSettings, nativeWindowSettings))
//         {
//             game.IsVisible = true;
//             game.Run();
//         }
//     }
// }