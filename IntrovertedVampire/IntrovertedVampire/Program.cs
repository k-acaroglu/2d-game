using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace FnafLike;

class Program
{
    static void Main()
    {
        using var game = new Core.Game(new NativeWindowSettings
        {
            Title = "Introverted Vampire",
            ClientSize = new Vector2i(1280, 720),
            Flags = ContextFlags.ForwardCompatible,
        });
        game.Run();
    }
}